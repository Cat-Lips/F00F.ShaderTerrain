namespace F00F;

public partial class Terrain
{
    private void InitCamera()
    {
        if (Camera.NotNull())
        {
            SetNearView();

            if (Config.NotNull())
            {
                SetFarView();
                Config.ChunksChanged += SetFarView;
            }
        }

        void SetNearView() => Camera.Near = Const.SmallFloat;
        void SetFarView() => Camera.Far = Config.ChunkSize * Config.ChunkRadius;
        // TODO: Haze/Fog relative to far clip distance
    }
}
