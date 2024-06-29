#if TOOLS
namespace F00F;

using Camera = Godot.Camera3D;

public partial class Terrain
{
    public sealed override void _Notification(int what)
    {
        if (Editor.OnPreSave(what))
        {
            if (Camera.NotNull())
            {
                Editor.DoPreSaveReset(Camera, Camera.PropertyName.Position);
                Editor.DoPreSaveReset(Camera, Camera.PropertyName.Near, .05f);
                Editor.DoPreSaveReset(Camera, Camera.PropertyName.Far, 4000);
            }

            if (this.IsEditedSceneRoot())
            {
                Editor.DoPreSaveResetField(this, PropertyName.Camera);
                Editor.DoPreSaveResetField(this, PropertyName.Config);
            }

            Mesh.ForEachChild(x => Editor.DoPreSaveReset(x, PropertyName.Owner));
            return;
        }

        if (Editor.OnPostSave(what))
            Editor.DoPostSaveRestore();
    }
}
#endif
