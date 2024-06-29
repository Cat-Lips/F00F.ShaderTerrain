#if TOOLS
using F00F;

namespace Tests;

using Camera = Camera3D;

public partial class Main
{
    protected sealed override void _OnEditorSave()
    {
        Editor.DoPreSaveReset(Camera, Camera.PropertyName.Position);
        Editor.DoPreSaveReset(Camera, Camera.PropertyName.Near, .05f);
        Editor.DoPreSaveReset(Camera, Camera.PropertyName.Far, 4000);

        Editor.DoPreSaveResetField(Terrain, Terrain.PropertyName.Config);
        Editor.DoPreSaveResetField(Terrain, Terrain.PropertyName.Camera);
    }
}
#endif
