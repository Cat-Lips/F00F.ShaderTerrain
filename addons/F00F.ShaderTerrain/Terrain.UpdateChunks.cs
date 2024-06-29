using Godot;

namespace F00F;

public partial class Terrain
{
    private Vector2 curPos;
    private Vector3? camPos;
    private void UpdateChunks()
    {
        if (Camera.IsNull()) return;

        var camPos = Camera.Position;
        if (this.camPos == camPos) return;
        this.camPos = camPos;

        this.ClampMax(Camera, Camera.Near);

        var curPos = camPos.XZ().Snapped(Config.ChunkSize);
        if (this.curPos == curPos) return;
        this.curPos = curPos;

        Mesh.Position = curPos.FromXZ();
        Config.SetShaderTerrainPos(curPos);
    }
}
