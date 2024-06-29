using System.Linq;
using Godot;

namespace F00F;

public static class TerrainTypeExtensions
{
    #region Get

    public static Texture2D[] Textures(this TerrainType[] regions)
        => regions.Select(x => x.Texture).ToArray();

    public static Color[] Tints(this TerrainType[] regions)
        => regions.Select(x => x.TextureTint).ToArray();

    public static float[] MinSlopes(this TerrainType[] regions)
        => regions.Select(x => x.MinSlope).ToArray();

    public static float[] MaxSlopes(this TerrainType[] regions)
        => regions.Select(x => x.MaxSlope).ToArray();

    public static float[] MinHeights(this TerrainType[] regions)
        => regions.Select(x => x.MinHeight).ToArray();

    public static float[] MaxHeights(this TerrainType[] regions)
        => regions.Select(x => x.MaxHeight).ToArray();

    public static float[] TextureScales(this TerrainType[] regions)
        => regions.Select(x => x.TextureScale).ToArray();

    public static float[] TintStrengths(this TerrainType[] regions)
        => regions.Select(x => x.TintStrength).ToArray();

    public static float[] BlendStrengths(this TerrainType[] regions)
        => regions.Select(x => x.BlendStrength).ToArray();

    #endregion

    #region Set

    public static void SetTextureScale(this TerrainType[] regions, float value)
        => regions.ForEach(x => x.TextureScale = value);

    public static void SetBlendStrength(this TerrainType[] regions, float value)
        => regions.ForEach(x => x.BlendStrength = value);

    public static void SetTintStrength(this TerrainType[] regions, float value)
        => regions.ForEach(x => x.TintStrength = value);

    public static void SetGradient(this TerrainType[] regions, float value)
        => regions.ForEach(x => x.Gradient = value);

    public static void SetLowerSmooth(this TerrainType[] regions, float value)
        => regions.ForEach(x => x.LowerSmooth = value);

    public static void SetUpperSmooth(this TerrainType[] regions, float value)
        => regions.ForEach(x => x.UpperSmooth = value);

    #endregion

    #region Avg

    public static float AvgTextureScale(this TerrainType[] regions)
        => regions.AverageOrDefault(x => x.TextureScale);

    public static float AvgBlendStrength(this TerrainType[] regions)
        => regions.AverageOrDefault(x => x.BlendStrength);

    public static float AvgTintStrength(this TerrainType[] regions)
        => regions.AverageOrDefault(x => x.TintStrength);

    public static float AvgGradient(this TerrainType[] regions)
        => regions.AverageOrDefault(x => x.Gradient);

    public static float AvgLowerSmooth(this TerrainType[] regions)
        => regions.AverageOrDefault(x => x.LowerSmooth);

    public static float AvgUpperSmooth(this TerrainType[] regions)
        => regions.AverageOrDefault(x => x.UpperSmooth);

    #endregion

    #region Reset

    public static void ResetSlopes(this TerrainType[] regions)
        => regions.ForEach(x => (x.MinSlope, x.MaxSlope) = (TerrainType.Default.MinSlope, TerrainType.Default.MaxSlope));

    public static void ResetHeights(this TerrainType[] regions)
        => regions.ForEach((x, idx, step) => (x.MinHeight, x.MaxHeight) = TerrainType.Utils.HeightRange(idx, step));

    public static void ResetGradients(this TerrainType[] regions)
        => regions.ForEach((x, idx, step) => (x.Gradient, x.LowerSmooth, x.UpperSmooth) = (TerrainType.Utils.MidHeight(idx, step), 0, 0));

    public static void ResetAverageTextureTints(this TerrainType[] regions)
        => regions.ForEach(x => x.SetAverageTextureTint());

    public static void ResetMostCommonTextureTints(this TerrainType[] regions)
        => regions.ForEach(x => x.SetMostCommonTextureTint());

    #endregion

    #region Utils

    public static void SetAverageTextureTint(this TerrainType region)
        => region.TextureTint = TerrainType.Utils.TextureTint(region.Texture, TerrainType.Utils.AverageTextureTint);

    public static void SetMostCommonTextureTint(this TerrainType region)
        => region.TextureTint = TerrainType.Utils.TextureTint(region.Texture, TerrainType.Utils.MostCommonTextureTint);

    #endregion
}
