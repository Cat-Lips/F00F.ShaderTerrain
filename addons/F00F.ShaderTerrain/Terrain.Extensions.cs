using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace F00F;

public static class TerrainExtensions
{
    #region Height

    public static bool PosAboveHeight(this Terrain terrain, in Vector3 pos)
        => terrain.PosAboveHeight(pos, out var _);

    public static bool PosBelowHeight(this Terrain terrain, in Vector3 pos)
        => terrain.PosBelowHeight(pos, out var _);

    public static bool PosAboveHeight(this Terrain terrain, in Vector3 pos, out float heightAboveTerrain)
        => (heightAboveTerrain = terrain.DistanceTo(pos)) > 0;

    public static bool PosBelowHeight(this Terrain terrain, in Vector3 pos, out float heightBelowTerrain)
        => (heightBelowTerrain = -terrain.DistanceTo(pos)) > 0;

    public static bool PosAboveMinHeight(this Terrain terrain, in Vector3 pos, float radius, out float heightAboveTerrain) => terrain.PosAboveHeight(pos, radius, HeightType.Min, out heightAboveTerrain);
    public static bool PosAboveMaxHeight(this Terrain terrain, in Vector3 pos, float radius, out float heightAboveTerrain) => terrain.PosAboveHeight(pos, radius, HeightType.Max, out heightAboveTerrain);
    public static bool PosAboveAvgHeight(this Terrain terrain, in Vector3 pos, float radius, out float heightAboveTerrain) => terrain.PosAboveHeight(pos, radius, HeightType.Avg, out heightAboveTerrain);
    private static bool PosAboveHeight(this Terrain terrain, in Vector3 pos, float radius, HeightType t, out float heightAboveTerrain)
        => (heightAboveTerrain = terrain.DistanceTo(pos, radius, t)) > 0;

    public static bool PosBelowMinHeight(this Terrain terrain, in Vector3 pos, float radius, out float heightBelowTerrain) => terrain.PosBelowHeight(pos, radius, HeightType.Min, out heightBelowTerrain);
    public static bool PosBelowMaxHeight(this Terrain terrain, in Vector3 pos, float radius, out float heightBelowTerrain) => terrain.PosBelowHeight(pos, radius, HeightType.Max, out heightBelowTerrain);
    public static bool PosBelowAvgHeight(this Terrain terrain, in Vector3 pos, float radius, out float heightBelowTerrain) => terrain.PosBelowHeight(pos, radius, HeightType.Avg, out heightBelowTerrain);
    private static bool PosBelowHeight(this Terrain terrain, in Vector3 pos, float radius, HeightType t, out float heightBelowTerrain)
        => (heightBelowTerrain = -terrain.DistanceTo(pos, radius, t)) > 0;

    public static float DistanceTo(this Terrain terrain, in Vector3 pos)
        => pos.Y - terrain.GetHeight(pos.X, pos.Z);

    public static float MinDistanceTo(this Terrain terrain, in Vector3 pos, float radius) => terrain.DistanceTo(pos, radius, HeightType.Min);
    public static float MaxDistanceTo(this Terrain terrain, in Vector3 pos, float radius) => terrain.DistanceTo(pos, radius, HeightType.Max);
    public static float AvgDistanceTo(this Terrain terrain, in Vector3 pos, float radius) => terrain.DistanceTo(pos, radius, HeightType.Avg);
    private static float DistanceTo(this Terrain terrain, in Vector3 pos, float radius, HeightType t)
        => pos.Y - terrain.GetHeight(pos.X, pos.Z, radius, t);

    #endregion

    #region Clamp

    public static void Clamp(this Terrain terrain, Node3D node)
    {
        var pos = node.Position;
        if (terrain.PosBelowHeight(pos, out var diff))
        {
            pos.Y += diff;
            node.Position = pos;
        }
    }

    public static void Clamp(this Terrain terrain, RigidBody3D body)
    {
        if (terrain.PosBelowHeight(body.Position, out var diff))
        {
            body.ApplyCentralForce(Vector3.Up * diff * body.Mass);
            body._DrawCentralForce(Vector3.Up * diff * body.Mass);
        }
    }

    public static void ClampMin(this Terrain terrain, Node3D node, float radius) => terrain.Clamp(node, radius, HeightType.Min);
    public static void ClampMax(this Terrain terrain, Node3D node, float radius) => terrain.Clamp(node, radius, HeightType.Max);
    public static void ClampAvg(this Terrain terrain, Node3D node, float radius) => terrain.Clamp(node, radius, HeightType.Avg);
    private static void Clamp(this Terrain terrain, Node3D node, float radius, HeightType t)
    {
        var pos = node.Position;
        if (terrain.PosBelowHeight(pos + Vector3.Down * radius, radius, t, out var diff))
        {
            pos.Y += diff;
            node.Position = pos;
        }
    }

    public static void ClampMin(this Terrain terrain, RigidBody3D body, float radius) => terrain.Clamp(body, radius, HeightType.Min);
    public static void ClampMax(this Terrain terrain, RigidBody3D body, float radius) => terrain.Clamp(body, radius, HeightType.Max);
    public static void ClampAvg(this Terrain terrain, RigidBody3D body, float radius) => terrain.Clamp(body, radius, HeightType.Avg);
    private static void Clamp(this Terrain terrain, RigidBody3D body, float radius, HeightType t)
    {
        if (terrain.PosBelowHeight(body.Position + Vector3.Down * radius, radius, t, out var diff))
        {
            body.ApplyCentralForce(Vector3.Up * diff * body.Mass);
            body._DrawCentralForce(Vector3.Up * diff * body.Mass);
        }
    }

    #endregion

    #region Private

    private enum HeightType { Min, Max, Avg }
    private static float GetHeight(this Terrain terrain, float x, float z, float radius, HeightType t)
    {
        return t switch
        {
            HeightType.Min => Heights().Min(),
            HeightType.Max => Heights().Max(),
            HeightType.Avg => Heights().Average(),
            _ => throw new NotImplementedException(),
        };

        IEnumerable<float> Heights()
        {
            foreach (var (xz, _) in Utils.Spiral(x, z, radius))
                yield return terrain.GetHeight(xz.X, xz.Y);
        }
    }

    #endregion
}
