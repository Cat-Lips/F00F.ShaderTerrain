using System;
using Godot;

namespace F00F;

using static TerrainConfig.Enum;
using ChunkId = Vector2;

[Tool, GlobalClass]
public partial class TerrainConfig : CustomResource
{
    #region Private

    public const string AssetsDir = "res://Assets/Terrain";
    public const string AssetsConfig = "res://Assets/Terrain/Regions.cfg";

    private readonly Shader Shader = Utils.LoadShader<Terrain>();
    private readonly ShaderMaterial ShaderMaterial = New.ShaderMaterial();

    #endregion

    #region Export

    public static class Enum
    {
        public enum ShapeType
        {
            Polygon,
            HeightMap,
        }

        public enum FallThruAction
        {
            None,
            Clamp,
            Delete,
            Custom,
        }
    }

    [ExportGroup("Terrain")]
    [Export] public ShaderNoise2D Noise { get; set => this.Set(ref field, value ?? new(), InitNoise); } = new();
    [Export] public int Amplitude { get; set => this.Set(ref field, value, OnAmplitudeSet); } = 100;

    [ExportGroup("Regions")]
    [Export(PropertyHint.Dir)] private string Assets { get; set => this.Set(ref field, value ?? AssetsDir, LoadAssets); } = AssetsDir;
    [Export(PropertyHint.SaveFile)] private string Config { get; set => this.Set(ref field, value ?? AssetsConfig, LoadAssets); } = AssetsConfig;
    [ExportToolButton(nameof(ReloadAssets))] private Callable ReloadAssets => Callable.From(LoadAssets);
    [ExportToolButton(nameof(ClearConfig))] private Callable ClearConfig => Callable.From(() => LoadAssets(reset: true));

    [Export] public TerrainType[] Regions { get; set => this.Set(ref field, value.NewIfNull(1), ResetRegions); }

    [Export] public bool UseBlending { get; set => this.Set(ref field, value, OnUseBlendingSet); } = true;
    [Export] public bool UseGradient { get; set => this.Set(ref field, value, notify: true, OnUseGradientSet); } = true;
    [Export] public bool UseSmoothing { get; set => this.Set(ref field, value, ResetGradientPreview); } = false;
    [Export] public Curve GradientPreview { get; set => ResetGradientPreview(); } = new();

#if TOOLS
    [ExportSubgroup("Tools")]
    [ExportToolButton(nameof(ResetSlopes))] private Callable ResetSlopes => Callable.From(Regions.ResetSlopes);
    [ExportToolButton(nameof(ResetHeights))] private Callable ResetHeights => Callable.From(Regions.ResetHeights);
    [ExportToolButton(nameof(ResetGradients))] private Callable ResetGradients => Callable.From(Regions.ResetGradients);
    [ExportToolButton(nameof(ResetAverageTints))] private Callable ResetAverageTints => Callable.From(Regions.ResetAverageTextureTints);
    [ExportToolButton(nameof(ResetMostCommonTints))] private Callable ResetMostCommonTints => Callable.From(Regions.ResetMostCommonTextureTints);

    [Export] private float GlobalTextureScale { get => Regions.AvgTextureScale(); set => Regions.SetTextureScale(value); }
    [Export(PropertyHint.Range, "0,1")] private float GlobalTintStrength { get => Regions.AvgTintStrength(); set => Regions.SetTintStrength(value); }
    [Export(PropertyHint.Range, "0,1")] private float GlobalBlendStrength { get => Regions.AvgBlendStrength(); set => Regions.SetBlendStrength(value); }

    [Export(PropertyHint.Range, "0,1")] private float GlobalGradient { get => Regions.AvgGradient(); set => Regions.SetGradient(value); }
    [Export(PropertyHint.Range, "-90,90,radians_as_degrees")] private float GlobalLowerSmooth { get => Regions.AvgLowerSmooth(); set => Regions.SetLowerSmooth(value); }
    [Export(PropertyHint.Range, "-90,90,radians_as_degrees")] private float GlobalUpperSmooth { get => Regions.AvgUpperSmooth(); set => Regions.SetUpperSmooth(value); }
#endif

    [ExportGroup("Mesh")]
    [Export] public int ChunkSize { get; set => this.Set(ref field, value.ToPo2(field), OnChunkSizeSet); } = Editor.IsEditor ? 64 : 256;
    [Export] public int ChunkRadius { get; set => this.Set(ref field, value.ClampMin(0), OnChunkRadiusSet); } = Editor.IsEditor ? 9 : 21;

    [ExportGroup("Body")]
    [Export] public int ShapeSize { get; set => this.Set(ref field, value.ClampMin(0), OnShapeChanged.Run); } = 32;
    [Export] public ShapeType ShapeType { get; set => this.Set(ref field, value, OnShapeChanged.Run); } = ShapeType.HeightMap;
    [Export] public FallThruAction FallThruAction { get; set; }

    #endregion

    public event Action ShapeChanged;
    public event Action ChunksChanged;

    public float GetHeight(float x, float z)
    {
        var height = GetHeight();
        var gradient = GetGradient();
        return height * gradient * Amplitude;

        float GetHeight()
            => (Noise.GetNoise(x, z) + 1f) * .5f;

        float GetGradient()
            => UseGradient ? GradientPreview.Sample(height) : 1;
    }

    public TerrainChunk CreateChunk(/*in */ChunkId id, int lod)
    {
        return Utils.New<TerrainChunk>(x =>
        {
            x.Name = $"Chunk {id}|{lod}";
            x.Position = (id * ChunkSize).FromXZ();
            x.ExtraCullMargin = Amplitude;
            x.Mesh = New.PlaneMesh(ChunkSize, lod, ShaderMaterial);
        });
    }

    public TerrainShape CreateShape(/*in */ChunkId id)
    {
        var pos = id * ShapeSize;
        var body = NewBody();
        var shape = NewShape();
        body.AddChild(shape, own: true);
        return body;

        TerrainShape NewBody()
        {
            return Utils.New<TerrainShape>(x =>
            {
                x.Name = $"Body {id}";
                x.Position = pos.FromXZ();
            });
        }

        CollisionShape3D NewShape()
        {
            return new()
            {
                Name = "Shape",
                Shape = ShapeType switch
                {
                    ShapeType.Polygon => New.PolygonShape(ShapeSize, pos, GetHeight),
                    ShapeType.HeightMap => New.HeightMapShape(ShapeSize, pos, GetHeight),
                    _ => throw new NotImplementedException(),
                }
            };
        }
    }

    #region Private

    private readonly AutoAction OnShapeChanged;
    private readonly AutoAction OnChunksChanged;

    public TerrainConfig()
    {
        OnShapeChanged = new(() => ShapeChanged?.Invoke());
        OnChunksChanged = new(() => ChunksChanged?.Invoke());

        InitNoise();
        InitSlope();
        InitShader();
        LoadAssets();

        void InitShader()
        {
            SetShaderAmplitude();
            SetShaderChunkSize();
        }
    }

    private void OnAmplitudeSet()
    {
        SetShaderAmplitude();
        OnShapeChanged.Run();
    }

    private void OnUseBlendingSet()
        => Noise.SetShaderDef(ShaderName.UseBlending, UseBlending);

    private void OnUseGradientSet()
        => Noise.SetShaderDef(ShaderName.UseGradient, UseGradient);

    private void OnChunkSizeSet()
    {
        SetShaderChunkSize();
        OnChunksChanged.Run();
    }

    private void OnChunkRadiusSet()
        => OnChunksChanged.Run();

    #endregion
}
