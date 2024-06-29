using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using F00F.ShaderNoise;
using Godot;

namespace F00F.ShaderTerrain
{
    [Tool, GlobalClass]
    public partial class TerrainData : Resource
    {
        #region Private

        private ShaderNoise2D _noise;
        private Texture2D _heightMap;
        private Texture2D _normalMap;
        private Texture2D _overlay;
        private float _amplitude = 75;

        private TerrainType[] _regions;
        private bool _enableBlending;
        private bool _useHeightCurve;
        private Curve _heightCurve;

        private int _lodStep = 1;
        private int _chunkSize = 512;
        private int _chunkRadius = 10;
        private float _chunkScale = 1;

        #endregion

        #region Export

        [ExportGroup("Terrain")]
        [Export] public ShaderNoise2D Noise { get => _noise; set => this.Set(ref _noise, value, ResetShader.Run, HeightValueChanged.Run, OnNoiseSet); }
        [Export] public Texture2D HeightMap { get => _heightMap; set => this.Set(ref _heightMap, value, SetShaderParam, ResetShader.Run, HeightValueChanged.Run, OnHeightMapSet); }
        [Export] public Texture2D NormalMap { get => _normalMap; set => this.Set(ref _normalMap, value, SetShaderParam, ResetShader.Run, HeightValueChanged.Run); }
        [Export] public Texture2D Overlay { get => _overlay; set => this.Set(ref _overlay, value, SetShaderParam, ResetShader.Run, HeightValueChanged.Run, OnOverlaySet); }
        [Export] public float Amplitude { get => _amplitude; set => this.Set(ref _amplitude, value, SetShaderParam, HeightValueChanged.Run); }

        [ExportGroup("Regions")]
        [Export] public TerrainType[] Regions { get => _regions; set => this.Set(ref _regions, TerrainType.Get(value), notify: true, ResetShader.Run, OnRegionsSet); }
        [Export] public bool EnableBlending { get => _enableBlending; set => this.Set(ref _enableBlending, value, ResetShader.Run); }
        [Export] public bool UseHeightCurve { get => _useHeightCurve; set => this.Set(ref _useHeightCurve, value, ResetShader.Run, HeightValueChanged.Run); }
        [Export] public Curve HeightCurve { get => _heightCurve; set => this.Set(ref _heightCurve, value ?? TerrainType.DefaultHeightCurve(Regions), HeightValueChanged.Run, OnHeightCurveSet); }

        [ExportSubgroup("Defaults", "Region")]
        [Export] public float RegionTextureScale { get => TerrainType.DefaultTextureScale; set => this.Set(ref TerrainType.DefaultTextureScale, value, () => Regions = null); }
        [Export(PropertyHint.Range, "0,1")] public float RegionTintStrength { get => TerrainType.DefaultTintStrength; set => this.Set(ref TerrainType.DefaultTintStrength, value, () => Regions = null); }
        [Export(PropertyHint.Range, "0,1")] public float RegionBlendStrength { get => TerrainType.DefaultBlendStrength; set => this.Set(ref TerrainType.DefaultBlendStrength, value, () => Regions = null); }
        [Export(PropertyHint.Range, "0,1")] public float RegionCurveTangent { get => TerrainType.DefaultCurveTangent; set => this.Set(ref TerrainType.DefaultCurveTangent, value, () => HeightCurve = null); }
        [Export] public bool RegionUseCurveTangents { get => TerrainType.UseCurveTangents; set => this.Set(ref TerrainType.UseCurveTangents, value, () => HeightCurve = null); }
        [Export] public DefaultTerrainType RegionDefaultType { get => TerrainType.DefaultTerrainTypes; set => this.Set(ref TerrainType.DefaultTerrainTypes, value, () => Regions = null); }
        [Export] public TintFromTexture RegionTintFromTexture { get => TerrainType.TintFromTexture; set => this.Set(ref TerrainType.TintFromTexture, value, () => Regions = null); }

        [ExportGroup("Chunks")]
        [Export] public int LodStep { get => _lodStep; set => this.Set(ref _lodStep, Mathf.Max(value, 0), SetShaderParam, ChunkValueChanged.Run); }
        [ExportGroup("Chunks", "Chunk")]
        [Export] public int ChunkSize { get => _chunkSize; set => this.Set(ref _chunkSize, Utils.NextPo2(value, _chunkSize), SetShaderParam, ChunkValueChanged.Run); }
        [Export] public int ChunkRadius { get => _chunkRadius; set => this.Set(ref _chunkRadius, Mathf.Max(value, 0), ChunkValueChanged.Run); }
        [Export] public float ChunkScale { get => _chunkScale; set => this.Set(ref _chunkScale, Mathf.Max(value, 0), SetShaderParam, HeightValueChanged.Run, ScaleValueChanged.Run); }

        #endregion

        public float ScaledChunkSize => ChunkSize * ChunkScale;
        public float ScaledTerrainSize => ScaledChunkSize * ChunkRadius * 2;

        public float GetHeight(float x, float z)
        {
            var height = RawHeight();
            var gradient = Gradient();
            return height * gradient * Amplitude * ChunkScale;

            float RawHeight()
            {
                return NoiseHeight() * ImageHeight() * OverlayHeight();

                float NoiseHeight()
                {
                    if (Noise is null) return 1;
                    var noise = Noise.GetNoise(x, z);
                    return (noise + 1) * .5f;
                }

                float ImageHeight()
                    => GetPixelR(heightImg, heightImgSize, heightImgOffset, x, z);

                float OverlayHeight()
                    => GetPixelR(overlayImg, overlayImgSize, overlayImgOffset, x, z);

                static float GetPixelR(Image image, in Vector2 size, in Vector2 offset, float x, float y)
                {
                    if (image is null) return 1;

                    x = Mathf.PosMod(x + offset.X, size.X);
                    y = Mathf.PosMod(y + offset.Y, size.Y);

                    return image.InterpolatePixel(x, y).R;
                }
            }

            float Gradient()
            {
                return UseHeightCurve ? HeightCurveGradient() : 1;

                float HeightCurveGradient()
                    => HeightCurve.Sample(height);
            }
        }

        #region Internal

        internal AutoAction ScaleValueChanged = new();
        internal AutoAction ChunkValueChanged = new();
        internal AutoAction HeightValueChanged = new();

        internal ShaderMaterial ShaderMaterial { get; } = new() { Shader = new() };

        internal static TerrainData Default() => new()
        {
            Noise = new(),
            Regions = null,
            HeightCurve = null,
            EnableBlending = true,
            UseHeightCurve = true,
        };

        #endregion

        #region Private

        private AutoAction ResetShader = new();
        private AutoAction ResetRegions = new();

        private Shader Shader { get; } = Utils.LoadShader<TerrainChunk>();

        private void OnShaderSet()
        {
            if (Shader is null) return;
            Shader.Changed += ResetShader.Run;
        }

        private void OnNoiseSet()
        {
            if (Noise is null) return;
            Noise.ShaderMaterial = ShaderMaterial;
            Noise.Changed += HeightValueChanged.Run;
        }

        private void OnRegionsSet()
        {
            ResetRegions.Run();
            foreach (var region in Regions.Where(x => x is not null))
                region.Connect(Resource.SignalName.Changed, ResetRegions.Run);
        }

        private void OnHeightCurveSet()
        {
            OnHeightCurveChanged();
            HeightCurve.Changed += OnHeightCurveChanged;

            void OnHeightCurveChanged()
            {
                var curve = new CurveTexture { Curve = HeightCurve };
                ShaderMaterial.SetShaderParameter("height_curve", curve);
            }
        }

        private Image heightImg;
        private Vector2 heightImgSize;
        private Vector2 heightImgOffset;
        private void OnHeightMapSet()
            => InitImg(HeightMap, out heightImg, out heightImgSize, out heightImgOffset);

        private Image overlayImg;
        private Vector2 overlayImgSize;
        private Vector2 overlayImgOffset;
        private void OnOverlaySet()
            => InitImg(Overlay, out overlayImg, out overlayImgSize, out overlayImgOffset);

        private void SetShaderParam([CallerMemberName] string member = null)
            => ShaderMaterial.SetShaderParameter(member.ToSnakeCase(), Get(member));

        private static void InitImg(Texture2D texture, out Image image, out Vector2 size, out Vector2 offset)
        {
            image = texture?.GetImage();
            if (image is null) { size = offset = default; return; }

            image.Decompress();
            size = image.GetSize();
            offset = size * .5f;
        }

        #endregion

        public TerrainData()
        {
            this.ResetShader.Action += ResetShader;
            this.ResetRegions.Action += ResetRegions;

            SetShaderDefaults();
            SetRegionDefaults();
            TrackChangesInEditor();
            this.ResetShader.Run();
            this.ResetRegions.Run();

            void ResetShader()
            {
                if (Noise is not null)
                    Noise.ShaderCode = string.Join("\n", Defs().Append(Shader.Code));
                else
                    ShaderMaterial.Shader.Code = string.Join("\n", Defs().Append(Shader.Code));

                IEnumerable<string> Defs()
                {
                    if (Noise is not null) yield return "#define USE_NOISE";
                    if (HeightMap is not null) yield return "#define USE_HEIGHTMAP";
                    if (HeightMap is not null && NormalMap is not null) yield return "#define USE_NORMALMAP";
                    if (Overlay is not null) yield return "#define USE_OVERLAY";
                    if (EnableBlending) yield return "#define USE_BLENDING";
                    if (UseHeightCurve) yield return "#define USE_GRADIENT";
                    yield return $"#define REGION_COUNT {Regions?.Count(x => x is not null) ?? 0}";
                    yield return string.Empty;
                }
            }

            void ResetRegions()
            {
                var regions = Regions.Where(x => x is not null).ToArray();

                SetParam(regions.Select(x => x.Tint).ToArray(), "tints");
                SetParam(regions.Select(x => x.Texture).ToArray(), "textures");
                SetParam(regions.Select(x => x.Gradient).ToArray(), "gradients");
                SetParam(regions.Select(x => x.MinSlope).ToArray(), "min_slopes");
                SetParam(regions.Select(x => x.MaxSlope).ToArray(), "max_slopes");
                SetParam(regions.Select(x => x.MinHeight).ToArray(), "min_heights");
                SetParam(regions.Select(x => x.MaxHeight).ToArray(), "max_heights");
                SetParam(regions.Select(x => x.TextureScale).ToArray(), "texture_scales");
                SetParam(regions.Select(x => x.TintStrength).ToArray(), "tint_strengths");
                SetParam(regions.Select(x => x.BlendStrength).ToArray(), "blend_strengths");

                void SetParam(Variant value, string name)
                    => ShaderMaterial.SetShaderParameter(name, value);
            }

            void SetRegionDefaults()
            {
                if (Regions?.Length is 0 or null)
                {
                    Regions = null;
                    HeightCurve = null;
                }
                else
                {
                    HeightCurve ??= null;
                }
            }

            void SetShaderDefaults()
            {
                SetParam(LodStep);
                SetParam(Amplitude);
                SetParam(ChunkSize);
                SetParam(ChunkScale);
                SetParam(HeightCurve);

                void SetParam<[MustBeVariant] T>(T _, [CallerArgumentExpression(nameof(_))] string member = null)
                    => SetShaderParam(member);
            }
        }
    }
}
