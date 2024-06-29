using System;
using Godot;

namespace F00F;

[Tool, GlobalClass]
public partial class TerrainType : CustomResource
{
    #region Actions

    public Action TextureSet { get; set; }
    public Action TextureScaleSet { get; set; }
    public Action BlendStrengthSet { get; set; }

    public Action TextureTintSet { get; set; }
    public Action TintStrengthSet { get; set; }

    public Action MinSlopeSet { get; set; }
    public Action MaxSlopeSet { get; set; }
    public Action MinHeightSet { get; set; }
    public Action MaxHeightSet { get; set; }

    public Action GradientSet { get; set; }

    #endregion

    #region Export

    [Export] public string Name { get; set; }

    [ExportGroup("Settings")]

    [ExportSubgroup("Texture")]
    [Export] public Texture2D Texture { get; set => this.Set(ref field, value, TextureSet); }
    [Export] public float TextureScale { get; set => this.Set(ref field, value.ClampMin(0), TextureScaleSet); } = Default.TextureScale;
    [Export(PropertyHint.Range, "0,1")] public float BlendStrength { get; set => this.Set(ref field, value, BlendStrengthSet); } = Default.BlendStrength;

    [ExportSubgroup("Tint")]
    [Export(PropertyHint.ColorNoAlpha)] public Color TextureTint { get; set => this.Set(ref field, value.With(a: 1), TextureTintSet); } = Default.TextureTint;
    [Export(PropertyHint.Range, "0,1")] public float TintStrength { get; set => this.Set(ref field, value, TintStrengthSet); } = Default.TintStrength;

    [ExportToolButton(nameof(AverageTextureTint))] private Callable AverageTextureTint => Callable.From(this.SetAverageTextureTint);
    [ExportToolButton(nameof(MostCommonTextureTint))] private Callable MostCommonTextureTint => Callable.From(this.SetMostCommonTextureTint);

    [ExportSubgroup("Scope")]
    [Export(PropertyHint.Range, "0,1")] public float MinSlope { get; set => this.Set(ref field, value, MinSlopeSet); } = Default.MinSlope;
    [Export(PropertyHint.Range, "0,1")] public float MaxSlope { get; set => this.Set(ref field, value, MaxSlopeSet); } = Default.MaxSlope;
    [Export(PropertyHint.Range, "0,1")] public float MinHeight { get; set => this.Set(ref field, value, MinHeightSet); } = Default.MinHeight;
    [Export(PropertyHint.Range, "0,1")] public float MaxHeight { get; set => this.Set(ref field, value, MaxHeightSet); } = Default.MaxHeight;

    [ExportSubgroup("Slope")]
    [Export(PropertyHint.Range, "0,1")] public float Gradient { get; set => this.Set(ref field, value, GradientSet); } = Default.Gradient;
    [Export(PropertyHint.Range, "-90,90,radians_as_degrees")] public float LowerSmooth { get; set => this.Set(ref field, value, GradientSet); } = Default.LowerSmooth;
    [Export(PropertyHint.Range, "-90,90,radians_as_degrees")] public float UpperSmooth { get; set => this.Set(ref field, value, GradientSet); } = Default.UpperSmooth;

    #endregion
}
