#if TOOLS

namespace F00F.ShaderTerrain.Tests
{
    public partial class Main
    {
        public override void _Notification(int what)
        {
            if (Editor.OnPreSave(what))
            {
                Editor.DoPreSaveReset(Camera, Camera.PropertyName._input);
                Editor.DoPreSaveReset(Camera, Camera.PropertyName._config);
                Editor.DoPreSaveReset(Camera, Camera.PropertyName.Position);
                Editor.DoPreSaveReset(Terrain, Terrain.PropertyName._config);
                return;
            }

            if (Editor.OnPostSave(what))
                Editor.DoPostSaveRestore();
        }
    }
}
#endif
