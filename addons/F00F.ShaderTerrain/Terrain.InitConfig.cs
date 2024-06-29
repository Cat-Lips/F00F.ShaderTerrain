namespace F00F;

public partial class Terrain
{
    private void InitConfig()
    {
        InitCamera();
        Config.ShapeChanged += ResetCamera;

        ResetShapes();
        Config.ShapeChanged += ResetShapes;

        ResetChunks();
        Config.ChunksChanged += ResetChunks;

        void ResetCamera()
            => camPos = null;

        void ResetShapes()
            => ShapesDirty = true;

        void ResetChunks()
        {
            Mesh.RemoveChildren();

            foreach (var (id, ring) in Utils.Spiral(Config.ChunkRadius))
            {
                var lod = (ring - 1).ClampMin(0);
                Mesh.AddChild(Config.CreateChunk(id, lod), own: true);
            }
        }
    }
}
