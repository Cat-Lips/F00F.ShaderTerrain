using System.Collections.Generic;
using System.Linq;
using F00F;
using Godot;

namespace Tests;

using UX = UI;

[Tool]
public partial class Main : Test3D
{
    private Terrain Terrain => field ??= GetNode<Terrain>("Terrain");
    private ColliderTest ColliderTest => field ??= Terrain.GetNode<ColliderTest>("ColliderTest");

    protected override void InitSettings()
        => Settings.Add("Terrain", Terrain.Config, x => Terrain.Config = x);

    protected override void AddOptions()
    {
        Options.Sep();
        Options.Add("ShowTerrainMesh", UX.Toggle("ShowTerrainMesh", Terrain.ShowMesh, on => Terrain.ShowMesh = on));
        Options.Add("EnableTerrainBody", UX.Toggle("EnableTerrainBody", Terrain.EnableBody, on => Terrain.EnableBody = on));
        Options.Sep();
        Options.Add("CamPos", () => Camera.Position.XZ().Rounded());
        Options.Add("CamHeightAboveTerrain", () => (Camera.Position.Y - Terrain.GetHeight(Camera.Position)).Rounded(3));
        Options.Sep();
        Options.Add("BodiesAboveTerrain", BodiesAboveTerrainStr);
        Options.Add("BodiesBelowTerrain", BodiesBelowTerrainStr);

        string BodiesAboveTerrainStr()
        {
            return string.Join(" ", Parts());

            IEnumerable<string> Parts()
            {
                yield return $"{BodiesAboveTerrain.Length}";

                if (BodiesAboveTerrain.Length is not 0)
                    yield return $"(Max Height: {BodiesAboveTerrain.Max(x => x.Position.Y).RoundInt()})";
            }
        }

        string BodiesBelowTerrainStr()
        {
            return string.Join(" ", Parts());

            IEnumerable<string> Parts()
            {
                yield return $"{BodiesBelowTerrain.Length}";

                if (BodiesBelowTerrain.Length is not 0)
                    yield return $"(Min Height: {BodiesBelowTerrain.Min(x => x.Position.Y).RoundInt()})";
            }
        }
    }

    #region Godot

    private TestBody[] BodiesAboveTerrain { get; set; }
    private TestBody[] BodiesBelowTerrain { get; set; }
    public sealed override void _Process(double delta)
    {
        var x = Terrain.GetChildren<TestBody>()
            .ToLookup(x => Terrain.PosAboveHeight(x.Position));
        BodiesAboveTerrain = [.. x[true]];
        BodiesBelowTerrain = [.. x[false]];
    }

    #endregion
}
