using System;
using System.Collections.Generic;
using Godot;

namespace F00F;

using static TerrainConfig.Enum;
using ControlPair = (Control Label, Control EditControl);

public partial class TerrainConfig : IEditable<TerrainConfig>
{
    public IEnumerable<ControlPair> GetEditControls() => GetEditControls(out var _);
    public IEnumerable<ControlPair> GetEditControls(out Action<TerrainConfig> SetData)
    {
        var ec = EditControls(out SetData);
        SetData(this);
        return ec;
    }

    public static IEnumerable<ControlPair> EditControls(out Action<TerrainConfig> SetData)
    {
        var noiseControls = ShaderNoise2D.EditControls(out var SetNoiseData);
        var regionControls = () => (TerrainType.EditControls(out var SetRegionData), SetRegionData);

        return UI.Create(out SetData, CreateUI);

        void CreateUI(UI.IBuilder ui)
        {
            ui.AddGroup("Terrain");
            ui.AddResource(nameof(Noise), fold: true/*, nullable: false*/, controls: noiseControls, SetData: SetNoiseData);
            ui.AddValue(nameof(Amplitude), @int: true);
            ui.EndGroup();

            ui.AddGroup("Regions");
            ui.AddArray(nameof(Regions), fold: true, GetItemControls: regionControls);
            ui.AddCheck(nameof(UseBlending));
            ui.AddCheck(nameof(UseGradient));
            ui.AddCheck(nameof(UseSmoothing));

            //ui.AddGroup("Globals", "Global");
            //ui.AddValue(nameof(GlobalTextureScale), range: (0, 1, null));
            //ui.AddValue(nameof(GlobalTintStrength), range: (0, 1, null));
            //ui.AddValue(nameof(GlobalBlendStrength), range: (0, 1, null));
            //ui.AddValue(nameof(GlobalGradient), range: (0, 1, null));
            //ui.AddValue(nameof(GlobalLowerSmooth), range: (0, 1, null));
            //ui.AddValue(nameof(GlobalUpperSmooth), range: (0, 1, null));
            //ui.EndGroup();

            ui.EndGroup();

            ui.AddGroup("Mesh");
            ui.AddValue(nameof(ChunkSize), @int: true);
            ui.AddValue(nameof(ChunkRadius), @int: true);
            ui.EndGroup();

            ui.AddGroup("Body");
            ui.AddValue(nameof(ShapeSize), @int: true);
            ui.AddOption(nameof(ShapeType), items: UI.Items<ShapeType>());
            ui.AddOption(nameof(FallThruAction), items: UI.Items<FallThruAction>());
            ui.EndGroup();
        }
    }
}
