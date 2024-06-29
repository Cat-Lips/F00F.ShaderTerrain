using System;
using Godot;

namespace F00F;

using MinMax = (float Min, float Max);

public partial class TerrainType
{
    internal static class Utils
    {
        public static float MinSlope(int idx, float step) => Default.MinSlope;
        public static float MaxSlope(int idx, float step) => Default.MaxSlope;

        public static float MinHeight(int idx, float step) => (idx * step).Rounded(3);
        public static float MaxHeight(int idx, float step) => ((idx + 1) * step).Rounded(3);
        public static float MidHeight(int idx, float step) => (MinHeight(idx, step) + MaxHeight(idx, step)) * .5f;
        public static MinMax HeightRange(int idx, float step) => (MinHeight(idx, step), MaxHeight(idx, step));

        public static float Gradient(int idx, float step) => MidHeight(idx, step);
        public static float LowerSmooth(int idx, float step) => Default.LowerSmooth;
        public static float UpperSmooth(int idx, float step) => Default.UpperSmooth;

        public static readonly Func<Image, Color> AverageTextureTint = img => img.AverageColor();
        public static readonly Func<Image, Color> MostCommonTextureTint = img => img.MostCommonColor();

        public static Func<Image, Color> DefaultTextureTint { get; set; } = AverageTextureTint;

        public static Color TextureTint(Texture2D texture) => TextureTint(texture, DefaultTextureTint);
        public static Color TextureTint(Texture2D texture, Func<Image, Color> GetColor)
        {
            if (texture is null)
                return Default.TextureTint;

            var img = texture.GetImage();
            if (img.IsCompressed()) img.Decompress();
            return GetColor(img);
        }
    }
}
