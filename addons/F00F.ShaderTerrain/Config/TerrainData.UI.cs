using System;
using System.Collections.Generic;
using F00F.ShaderNoise;
using Godot;
using ControlPair = (Godot.Control Label, Godot.Control EditControl);

namespace F00F.ShaderTerrain
{
    public partial class TerrainData
    {
        public static IEnumerable<ControlPair> GetEditControls(out Action<TerrainData> SetTerrainData, bool all = true)
        {
            var noiseControls = ShaderNoise2D.GetEditControls(out var SetNoiseData, all);
            var regionControls = () => (TerrainType.GetEditControls(out var SetRegionData), SetRegionData);
            return UI.Create(out SetTerrainData, CreateUI, CustomiseUI, !all ? HideControlsThatTriggerShaderCompile : null);

            void CreateUI(UI.IBuilder ui)
            {
                ui.AddGroup("Terrain");
                ui.AddResource(nameof(Noise), nullable: all, controls: noiseControls, SetData: SetNoiseData);
                ui.AddTexture(nameof(HeightMap));
                ui.AddTexture(nameof(NormalMap));
                ui.AddTexture(nameof(Overlay));
                ui.AddValue(nameof(Amplitude));
                ui.EndGroup();

                ui.AddGroup("Regions");
                ui.AddArray(nameof(Regions), GetItemControls: regionControls);
                ui.AddCheck(nameof(EnableBlending));
                ui.AddCheck(nameof(UseHeightCurve));
                ui.AddCurve(nameof(HeightCurve));

                ui.AddGroup("Defaults", prefix: "Region");
                ui.AddValue(nameof(RegionTextureScale), range: (0, null, null));
                ui.AddValue(nameof(RegionTintStrength), range: (0, 1, null));
                ui.AddValue(nameof(RegionBlendStrength), range: (0, 1, null));
                ui.AddValue(nameof(RegionCurveTangent));
                ui.AddCheck(nameof(RegionUseCurveTangents));
                ui.AddOption(nameof(RegionDefaultType), items: UI.Items<DefaultTerrainType>());
                ui.AddOption(nameof(RegionTintFromTexture), items: UI.Items<TintFromTexture>());
                ui.EndGroup();
                ui.EndGroup();

                ui.AddGroup("Chunks", prefix: "Chunk");
                ui.AddValue(nameof(LodStep), range: (1, null, null), @int: true);
                ui.AddValue(nameof(ChunkSize), range: (0, null, null), @int: true);
                ui.AddValue(nameof(ChunkRadius), range: (0, null, null), @int: true);
                ui.AddValue(nameof(ChunkScale), range: (0, null, .01f));
                ui.EndGroup();
            }

            void CustomiseUI(UI.IBuilder ui)
            {
                HideNormalMap();
                HideHeightCurve();

                void HideNormalMap()
                {
                    var heightMapEdit = ui.GetEditControlComponent<LineEdit>(nameof(HeightMap));
                    var normalMapControls = ui.GetControls(nameof(NormalMap));

                    SetVisibility();
                    heightMapEdit.TextChanged += x => SetVisibility();

                    void SetVisibility()
                    {
                        var visible = !string.IsNullOrEmpty(heightMapEdit.Text);
                        normalMapControls.ForEach(x => x.Visible = visible);
                    }
                }

                void HideHeightCurve()
                {
                    var useHeightCurve = ui.GetEditControl<Button>(nameof(UseHeightCurve));
                    var heightCurveControls = ui.GetControls(nameof(HeightCurve));

                    SetVisibility(useHeightCurve.ButtonPressed);
                    useHeightCurve.Toggled += SetVisibility;

                    void SetVisibility(bool on)
                        => heightCurveControls.ForEach(x => x.Visible = on);
                }
            }

            static void HideControlsThatTriggerShaderCompile(UI.IBuilder ui)
            {
                var optionControls = ui.GetControls(
                    nameof(HeightMap),
                    nameof(NormalMap),
                    nameof(Overlay),
                    nameof(Regions),
                    nameof(EnableBlending),
                    nameof(UseHeightCurve));

                optionControls.ForEach(x => x.Visible = false);
            }
        }
    }
}
