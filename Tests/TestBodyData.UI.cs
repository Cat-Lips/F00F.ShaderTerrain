using System;
using System.Collections.Generic;
using Godot;
using ControlPair = (Godot.Control Label, Godot.Control EditControl);

namespace F00F.ShaderTerrain.Tests
{
    public partial class TestBodyData
    {
        public static IEnumerable<ControlPair> GetEditControls(out Action<TestBodyData> SetData, out Action<bool> EnableOptions)
        {
            Action<bool> EnableOptionsFunc = null;
            var controls = UI.Create(out SetData, CreateUI, CustomiseUI);
            EnableOptions = EnableOptionsFunc;
            return controls;

            static void CreateUI(UI.IBuilder ui)
            {
                ui.AddValue(nameof(ShapeSize), range: (0, 10, .1f));
                ui.AddOption(nameof(ShapeType), items: UI.Items<ShapeType>());
                ui.AddOption(nameof(ColliderType), items: UI.Items<ShaderTerrain.ShapeType>());
            }

            void CustomiseUI(UI.IBuilder ui)
            {
                var shapeSizeEdit = ui.GetEditControl<SpinBox>(nameof(ShapeSize));
                var shapeTypeEdit = ui.GetEditControl<OptionButton>(nameof(ShapeType));
                var colliderTypeEdit = ui.GetEditControl<OptionButton>(nameof(ColliderType));

                EnableOptionsFunc = EnableOptions;

                void EnableOptions(bool enable)
                {
                    shapeSizeEdit.Editable = enable;
                    shapeTypeEdit.Disabled = !enable;
                    colliderTypeEdit.Disabled = !enable;
                }
            }
        }
    }
}
