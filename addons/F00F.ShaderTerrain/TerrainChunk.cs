using System.Diagnostics;
using Godot;

namespace F00F.ShaderTerrain
{
    [Tool]
    internal partial class TerrainChunk : MeshInstance3D
    {
        #region Instantiate

        private static PackedScene _scene;
        private static PackedScene Scene => _scene ??= Utils.LoadScene<TerrainChunk>();

        public static TerrainChunk Instantiate(TerrainData config, float x, float z, int lod)
        {
            var scene = Scene.Instantiate<TerrainChunk>();
            scene.Initialise(config, x, z, lod);
            return scene;
        }

        #endregion

        private void Initialise(TerrainData config, float x, float z, int lod)
        {
            Name = $"Chunk ({x},{z}|{lod})";
            Position = new Vector3(x, 0, z) * config.ScaledChunkSize;
            Mesh = CreatePlane(config.ChunkSize, lod * config.LodStep, config.ShaderMaterial);
            ExtraCullMargin = config.Amplitude;

            static PlaneMesh CreatePlane(int size, int lod, Material material)
            {
                Debug.Assert(size == Mathf.NearestPo2(size));

                lod = lod <= 0
                    ? size - 1
                    : size / (int)Mathf.Pow(2, lod) - 1;

                return new()
                {
                    Size = Vector2.One * size,
                    SubdivideDepth = lod,
                    SubdivideWidth = lod,
                    Material = material,
                };
            }
        }
    }
}
