using System.Collections.Generic;
using System.Linq;
using Godot;

namespace F00F.ShaderTerrain
{
    public static class TerrainExtensions
    {
        public static IEnumerable<float> GetHeights(this Terrain terrain, float x, float z, float radius)
        {
            foreach (var (xz, _) in Utils.Spiral(x, z, radius))
                yield return terrain.GetHeight(xz.X, xz.Y);
        }

        public static void Clamp(this Terrain terrain, Node3D node, float radius)
        {
            var gpos = node.GlobalPosition;
            var terrainHeight = terrain.GetHeights(gpos.X, gpos.Z, radius).Max();
            var requiredHeight = terrainHeight + radius;
            if (gpos.Y < requiredHeight)
            {
                gpos.Y = requiredHeight;
                node.GlobalPosition = gpos;
            }
        }
    }
}
