using Godot;

namespace F00F;

public partial class TerrainType
{
    internal static class Default
    {
        public static readonly Color TextureTint = new(0, 0, 0, 1);

        public const float TextureScale = 100;
        public const float BlendStrength = 1f;
        public const float TintStrength = 0;

        public const float MinSlope = 0;
        public const float MaxSlope = 1;
        public const float MinHeight = 0;
        public const float MaxHeight = 1;

        public const float Gradient = .5f;
        public const float LowerSmooth = 0;
        public const float UpperSmooth = 0;
    }
}
