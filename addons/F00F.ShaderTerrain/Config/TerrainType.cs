using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using CurvePoint = (Godot.Vector2 Point, float TangentLeft, float TangentRight);

namespace F00F.ShaderTerrain
{
    public enum DefaultTerrainType
    {
        Height,
        Slope,
        Mixed,
    }

    public enum TintFromTexture
    {
        None,
        Average,
        MostCommon,
    }

    [Tool, GlobalClass]
    public partial class TerrainType : Resource
    {
        #region Defaults

        internal static DefaultTerrainType DefaultTerrainTypes = DefaultTerrainType.Height;
        internal static TintFromTexture TintFromTexture = TintFromTexture.None;

        internal static float DefaultTextureScale = 25;
        internal static float DefaultTintStrength = 0;
        internal static float DefaultBlendStrength = .5f;
        internal static float DefaultCurveTangent = 45;
        internal static bool UseCurveTangents = true;

        internal static bool IsDefault { get; private set; }
        internal static TerrainType[] Get(TerrainType[] value)
            => (IsDefault = value is null) ? LoadDefaults().ToArray() : value;

        #endregion

        #region Private

        private string _name;

        private Color _tint;
        private Texture2D _texture;
        private float _gradient = 1;

        private float _minSlope = 0;
        private float _maxSlope = 1;
        private float _minHeight = 0;
        private float _maxHeight = 1;

        private float _textureScale = DefaultTextureScale;
        private float _tintStrength = DefaultTintStrength;
        private float _blendStrength = DefaultBlendStrength;

        #endregion

        #region Export

        [Export] public string Name { get => _name; set => this.Set(ref _name, value); }

        [Export(PropertyHint.ColorNoAlpha)] public Color Tint { get => _tint; set => this.Set(ref _tint, value); }
        [Export] public Texture2D Texture { get => _texture; set => this.Set(ref _texture, value); }
        [Export(PropertyHint.Range, "0,1")] public float Gradient { get => _gradient; set => this.Set(ref _gradient, value); }

        [Export(PropertyHint.Range, "0,1")] public float MinSlope { get => _minSlope; set => this.Set(ref _minSlope, Mathf.Clamp(value, 0, MaxSlope)); }
        [Export(PropertyHint.Range, "0,1")] public float MaxSlope { get => _maxSlope; set => this.Set(ref _maxSlope, Mathf.Clamp(value, MinSlope, 1)); }
        [Export(PropertyHint.Range, "0,1")] public float MinHeight { get => _minHeight; set => this.Set(ref _minHeight, Mathf.Clamp(value, 0, MaxHeight)); }
        [Export(PropertyHint.Range, "0,1")] public float MaxHeight { get => _maxHeight; set => this.Set(ref _maxHeight, Mathf.Clamp(value, MinHeight, 1)); }

        [Export] public float TextureScale { get => _textureScale; set => this.Set(ref _textureScale, value); }
        [Export(PropertyHint.Range, "0,1")] public float TintStrength { get => _tintStrength; set => this.Set(ref _tintStrength, value); }
        [Export(PropertyHint.Range, "0,1")] public float BlendStrength { get => _blendStrength; set => this.Set(ref _blendStrength, value); }

        #endregion

        #region Internal

        internal static Curve DefaultHeightCurve(IEnumerable<TerrainType> regions)
        {
            var curve = new Curve();
            AddCurvePoints();
            return curve;

            void AddCurvePoints()
            {
                if (UseCurveTangents)
                {
                    foreach (var (point, tangentLeft, tangentRight) in CurvePoints())
                        curve.AddPoint(point, tangentLeft, tangentRight);
                }
                else
                {
                    foreach (var (point, _, _) in CurvePoints())
                        curve.AddPoint(point, leftMode: Curve.TangentMode.Linear, rightMode: Curve.TangentMode.Linear);
                }

                IEnumerable<CurvePoint> CurvePoints()
                    => Normalise(RegionPoints(out var yMin, out var yMax), yMin, yMax);

                Vector2[] RegionPoints(out float yMin, out float yMax)
                {
                    var minHeight = 0f;
                    var maxHeight = 0f;
                    var points = Points().ToArray();
                    yMin = minHeight;
                    yMax = maxHeight;
                    return points;

                    IEnumerable<Vector2> Points()
                    {
                        yield return Vector2.Zero;

                        var lastHeight = 0f;
                        foreach (var (height, gradient) in Heights()
                            .DistinctBy(x => x.Height)
                            .OrderBy(x => x.Height))
                        {
                            var newHeight = lastHeight + height * gradient;
                            yield return new(height, newHeight);
                            lastHeight = newHeight;

                            minHeight = Mathf.Min(minHeight, newHeight);
                            maxHeight = Mathf.Max(maxHeight, newHeight);
                        }

                        IEnumerable<(float Height, float Gradient)> Heights()
                            => regions.Select(x => (x.MaxHeight, x.Gradient));
                    }
                }

                IEnumerable<CurvePoint> Normalise(Vector2[] points, float yMin, float yMax)
                {
                    var lastHeight = 0f;
                    var baseTangent = Mathf.DegToRad(DefaultCurveTangent);

                    Debug.Assert(yMin is 0);
                    foreach (var p in points)
                    {
                        var height = NormalisedHeight(p.Y, yMax);
                        var tangent = baseTangent * (height - lastHeight);
                        yield return (new(p.X, height), -tangent, +tangent);
                        lastHeight = height;
                    }

                    static float NormalisedHeight(float y, float yMax)
                        => y is 0 ? 0 : y / yMax;
                }
            }
        }

        #endregion
    }
}
