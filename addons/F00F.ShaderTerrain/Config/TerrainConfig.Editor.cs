#if TOOLS
using Godot.Collections;

namespace F00F;

public partial class TerrainConfig
{
    public sealed override void _ValidateProperty(Dictionary property)
    {
        if (Editor.Show(property, PropertyName.UseSmoothing, @if: UseGradient)) return;
        if (Editor.Show(property, PropertyName.GradientPreview, @if: UseGradient)) return;

        if (Editor.SetDisplayOnly(property, PropertyName.GlobalTextureScale)) return;
        if (Editor.SetDisplayOnly(property, PropertyName.GlobalTintStrength)) return;
        if (Editor.SetDisplayOnly(property, PropertyName.GlobalBlendStrength)) return;

        if (Editor.SetDisplayOnly(property, PropertyName.GlobalGradient)) return;
        if (Editor.SetDisplayOnly(property, PropertyName.GlobalLowerSmooth)) return;
        if (Editor.SetDisplayOnly(property, PropertyName.GlobalUpperSmooth)) return;
    }
}
#endif
