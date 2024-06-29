using System;
using Godot;

namespace F00F.ShaderTerrain.Tests
{
    [Tool]
    public partial class Options : Stats
    {
        protected override void OnReady()
        {
            base.OnReady();

            AddSep();
            AddOptions();
            AddSep();

            void AddOptions()
            {
                Grid.Init(TestBodyData.GetEditControls(out var SetData, out _EnableOptions));
                Add("ShapeCount", () => TestBody.ShapeCount);
                Add("DropCount", () => TestBody.DropCount);
                SetData(TestBody.Config);
            }
        }

        private Action<bool> _EnableOptions;
        public void EnableOptions(bool enable)
            => _EnableOptions(enable);
    }
}
