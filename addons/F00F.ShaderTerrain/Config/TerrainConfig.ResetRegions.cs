namespace F00F;

public partial class TerrainConfig
{
    private void ResetRegions()
    {
        InitRegions();
        SetShaderParams();
        ResetGradientPreview();
        Noise.SetShaderDef(ShaderName.RegionCount, Regions.Length);

        void InitRegions() => Regions.ForEach(x =>
        {
            x.TextureSet = SetShaderTextures;
            x.TextureScaleSet = SetShaderTextureScales;
            x.BlendStrengthSet = SetShaderBlendStrengths;

            x.TextureTintSet = SetShaderTints;
            x.TintStrengthSet = SetShaderTintStrengths;

            x.MinSlopeSet = SetShaderMinSlopes;
            x.MaxSlopeSet = SetShaderMaxSlopes;
            x.MinHeightSet = SetShaderMinHeights;
            x.MaxHeightSet = SetShaderMaxHeights;

            x.GradientSet = ResetGradientPreview;
        });

        void SetShaderParams()
        {
            SetShaderTints();
            SetShaderTextures();
            SetShaderMinSlopes();
            SetShaderMaxSlopes();
            SetShaderMinHeights();
            SetShaderMaxHeights();
            SetShaderTextureScales();
            SetShaderTintStrengths();
            SetShaderBlendStrengths();

            SetShaderRegionCount();
        }
    }
}
