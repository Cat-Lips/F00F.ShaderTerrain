using System;
using System.Collections.Generic;
using Godot;

namespace F00F;

using ControlPair = (Control Label, Control EditControl);

public partial class TerrainType : IEditable<TerrainType>
{
    public IEnumerable<ControlPair> GetEditControls() => GetEditControls(out var _);
    public IEnumerable<ControlPair> GetEditControls(out Action<TerrainType> SetData)
    {
        var ec = EditControls(out SetData);
        SetData(this);
        return ec;
    }

    public static IEnumerable<ControlPair> EditControls(out Action<TerrainType> SetData)
    {
        return UI.Create(out SetData, CreateUI);

        static void CreateUI(UI.IBuilder ui)
        {
            ui.AddText(nameof(Name));

            ui.AddGroup("Texture");
            ui.AddTexture(nameof(Texture));
            ui.AddValue(nameof(TextureScale));
            ui.AddValue(nameof(BlendStrength), range: (0, 1, null));
            ui.EndGroup();

            ui.AddGroup("Tint");
            ui.AddColor(nameof(TextureTint));
            ui.AddValue(nameof(TintStrength), range: (0, 1, null));
            ui.EndGroup();

            ui.AddGroup("Scope");
            ui.AddValue(nameof(MinSlope), range: (0, 1, null));
            ui.AddValue(nameof(MaxSlope), range: (0, 1, null));
            ui.AddValue(nameof(MinHeight), range: (0, 1, null));
            ui.AddValue(nameof(MaxHeight), range: (0, 1, null));
            ui.EndGroup();

            ui.AddGroup("Slope");
            ui.AddValue(nameof(Gradient), range: (0, 1, null));
            ui.AddValue(nameof(LowerSmooth));
            ui.AddValue(nameof(UpperSmooth));
            ui.EndGroup();
        }
    }
}
