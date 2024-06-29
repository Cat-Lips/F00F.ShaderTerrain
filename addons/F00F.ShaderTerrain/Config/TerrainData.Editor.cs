#if TOOLS
using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace F00F.ShaderTerrain
{
    public partial class TerrainData
    {
        public override void _ValidateProperty(Dictionary property)
        {
            if (Editor.Show(property, PropertyName.NormalMap, @if: HeightMap is not null)) return;
            if (Editor.Show(property, PropertyName.HeightCurve, @if: UseHeightCurve)) return;

            if (Editor.SetDisplayOnly(property, PropertyName.RegionTintStrength)) return;
            if (Editor.SetDisplayOnly(property, PropertyName.RegionTextureScale)) return;
            if (Editor.SetDisplayOnly(property, PropertyName.RegionBlendStrength)) return;
            if (Editor.SetDisplayOnly(property, PropertyName.RegionCurveTangent)) return;

            if (Editor.SetDisplayOnly(property, PropertyName.RegionDefaultType)) return;
            if (Editor.SetDisplayOnly(property, PropertyName.RegionTintFromTexture)) return;
        }
    }
}
#endif
