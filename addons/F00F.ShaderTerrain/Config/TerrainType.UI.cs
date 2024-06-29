using System;
using System.Collections.Generic;
using ControlPair = (Godot.Control Label, Godot.Control EditControl);

namespace F00F.ShaderTerrain
{
    public partial class TerrainType
    {
        public static IEnumerable<ControlPair> GetEditControls(out Action<TerrainType> SetRegionData)
        {
            return UI.Create(out SetRegionData, CreateUI);

            static void CreateUI(UI.IBuilder ui)
            {
                ui.AddText(nameof(Name));

                ui.AddColor(nameof(Tint));
                ui.AddTexture(nameof(Texture));
                ui.AddValue(nameof(Gradient), range: (0, 1, null));

                ui.AddValue(nameof(MinSlope), range: (0, 1, null));
                ui.AddValue(nameof(MaxSlope), range: (0, 1, null));
                ui.AddValue(nameof(MinHeight), range: (0, 1, null));
                ui.AddValue(nameof(MaxHeight), range: (0, 1, null));

                ui.AddValue(nameof(TextureScale), range: (0, null, null));
                ui.AddValue(nameof(TintStrength), range: (0, 1, null));
                ui.AddValue(nameof(BlendStrength), range: (0, 1, null));
            }
        }
    }
}
