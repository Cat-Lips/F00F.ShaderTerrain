using Godot;

namespace F00F;

public partial class TerrainConfig
{
    private void SetShaderTints() => ShaderMaterial.SetShaderParameter(ShaderName.Tints, Regions.Tints());
    private void SetShaderTextures() => ShaderMaterial.SetShaderParameter(ShaderName.Textures, Regions.Textures());
    private void SetShaderMinSlopes() => ShaderMaterial.SetShaderParameter(ShaderName.MinSlopes, Regions.MinSlopes());
    private void SetShaderMaxSlopes() => ShaderMaterial.SetShaderParameter(ShaderName.MaxSlopes, Regions.MaxSlopes());
    private void SetShaderMinHeights() => ShaderMaterial.SetShaderParameter(ShaderName.MinHeights, Regions.MinHeights());
    private void SetShaderMaxHeights() => ShaderMaterial.SetShaderParameter(ShaderName.MaxHeights, Regions.MaxHeights());
    private void SetShaderTextureScales() => ShaderMaterial.SetShaderParameter(ShaderName.TextureScales, Regions.TextureScales());
    private void SetShaderTintStrengths() => ShaderMaterial.SetShaderParameter(ShaderName.TintStrengths, Regions.TintStrengths());
    private void SetShaderBlendStrengths() => ShaderMaterial.SetShaderParameter(ShaderName.BlendStrengths, Regions.BlendStrengths());

    private void SetShaderRegionCount() => ShaderMaterial.SetShaderParameter(ShaderName.RegionLength, Regions.Length.ClampMin(1));

    private void SetShaderHeightCurve() => ShaderMaterial.SetShaderParameter(ShaderName.HeightCurve, HeightCurve);
    //private void SetShaderHeightMap() => ShaderMaterial.SetShaderParameter(ShaderName.HeightMap, HeightMap);
    //private void SetShaderNormalMap() => ShaderMaterial.SetShaderParameter(ShaderName.NormalMap, NormalMap);
    //private void SetShaderOverlay() => ShaderMaterial.SetShaderParameter(ShaderName.Overlay, Overlay);

    private void SetShaderAmplitude() => ShaderMaterial.SetShaderParameter(ShaderName.Amplitude, Amplitude);
    private void SetShaderChunkSize() => ShaderMaterial.SetShaderParameter(ShaderName.ChunkSize, ChunkSize);
    internal void SetShaderTerrainPos(in Vector2 pos) => ShaderMaterial.SetShaderParameter(ShaderName.TerrainPos, pos);

    private static class ShaderName
    {
        public static readonly StringName UseBlending = "USE_BLENDING";
        public static readonly StringName UseGradient = "USE_GRADIENT";
        public static readonly StringName RegionCount = "REGION_COUNT";

        public static readonly StringName Tints = "tints";
        public static readonly StringName Textures = "textures";
        public static readonly StringName MinSlopes = "min_slopes";
        public static readonly StringName MaxSlopes = "max_slopes";
        public static readonly StringName MinHeights = "min_heights";
        public static readonly StringName MaxHeights = "max_heights";
        public static readonly StringName TextureScales = "texture_scales";
        public static readonly StringName TintStrengths = "tint_strengths";
        public static readonly StringName BlendStrengths = "blend_strengths";

        public static readonly StringName RegionLength = "region_count";

        public static readonly StringName HeightCurve = "height_curve";
        //public static readonly StringName HeightMap = "height_map";
        //public static readonly StringName NormalMap = "normal_map";
        //public static readonly StringName Overlay = "overlay";

        public static readonly StringName Amplitude = "amplitude";
        public static readonly StringName ChunkSize = "chunk_size";
        public static readonly StringName TerrainPos = "terrain_pos";
    }
}
