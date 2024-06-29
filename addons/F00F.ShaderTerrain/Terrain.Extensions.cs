using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace F00F;

public static class TerrainExtensions
{
    #region Ray

    public static bool CastRay(this Terrain terrain, out Vector3 hit, float step = 1f, float tolerance = .01f)
    {
        var camera = terrain.GetViewport().GetCamera3D();
        var screenPos = terrain.GetViewport().GetMousePosition();
        var rayStart = camera.ProjectRayOrigin(screenPos);
        var rayNormal = camera.ProjectRayNormal(screenPos);
        return terrain.CastRay(rayStart, rayNormal, out hit, camera.Far, step, tolerance);
    }

    public static bool CastRay(this Terrain terrain, in Vector3 origin, in Vector3 direction, out Vector3 hit, float? range = null, float step = 1f, float tolerance = .01f)
    {
        if (PointBelowTerrain(origin, out var height))
        {
            hit = origin.With(y: height);
            return false;
        }

        if (PointOnTerrain(origin, height))
        {
            hit = origin;
            return true;
        }

        var curDist = 0f;
        var maxHeight = terrain.Config.Amplitude;
        var rayIsUpOrFlat = direction.Y >= 0;

        if (AboveMaxHeight(origin.Y))
        {
            if (rayIsUpOrFlat)
            {
                hit = default;
                return false;
            }

            curDist = (origin.Y - maxHeight) / -direction.Y;

            if (RangeComplete())
            {
                hit = default;
                return false;
            }
        }

        while (true)
        {
            var curPos = origin + direction * curDist;

            if (rayIsUpOrFlat && AboveMaxHeight(curPos.Y))
            {
                hit = default;
                return false;
            }

            if (PointBelowTerrain(curPos, out height))
            {
                var min = curDist - step;
                var max = curDist;

                while (true)
                {
                    var midDist = (min + max) * .5f;
                    var midPos = origin + direction * midDist;

                    if (max - min <= tolerance)
                    {
                        hit = midPos;
                        return true;
                    }

                    if (PointBelowTerrain(midPos, out height))
                    {
                        max = midDist;
                        continue;
                    }

                    if (PointOnTerrain(midPos, height))
                    {
                        hit = midPos;
                        return true;
                    }

                    min = midDist;
                }
            }

            if (PointOnTerrain(curPos, height))
            {
                hit = curPos;
                return true;
            }

            curDist += step;

            if (RangeComplete())
            {
                hit = default;
                return false;
            }
        }

        bool PointBelowTerrain(in Vector3 point, out float surface)
        {
            surface = terrain.GetHeight(point);
            return point.Y < surface - tolerance;
        }

        bool PointOnTerrain(in Vector3 point, float surface)
            => Mathf.IsEqualApprox(point.Y, surface, tolerance);

        bool AboveMaxHeight(float height)
            => height > maxHeight;

        bool RangeComplete()
            => curDist >= range;
    }

    public static Vector3 GetNormal(this Terrain terrain, in Vector3 pos)
    {
        var east_vertex = pos.Add(x: 1);
        var west_vertex = pos.Add(x: -1);
        var north_vertex = pos.Add(z: 1);
        var south_vertex = pos.Add(z: -1);

        east_vertex.Y = terrain.GetHeight(east_vertex);
        west_vertex.Y = terrain.GetHeight(west_vertex);
        north_vertex.Y = terrain.GetHeight(north_vertex);
        south_vertex.Y = terrain.GetHeight(south_vertex);

        var dx = east_vertex - west_vertex;
        var dy = north_vertex - south_vertex;
        return dy.Cross(dx).Normalized();
    }

    public static Vector3 GetMinHeight(this Terrain terrain, in Vector3 pos, float radius) => pos.With(y: terrain.GetHeight(pos.X, pos.Z, radius, HeightType.Min));
    public static Vector3 GetMaxHeight(this Terrain terrain, in Vector3 pos, float radius) => pos.With(y: terrain.GetHeight(pos.X, pos.Z, radius, HeightType.Max));
    public static Vector3 GetAvgHeight(this Terrain terrain, in Vector3 pos, float radius) => pos.With(y: terrain.GetHeight(pos.X, pos.Z, radius, HeightType.Avg));

    public static Transform3D Align(this Terrain terrain, in Vector3 pos, in Vector3 lookAt) => terrain.Align(pos, terrain.GetNormal(pos), lookAt);
    public static Transform3D Align(this Terrain _, in Vector3 pos, in Vector3 up, in Vector3 lookAt)
    {
        var lookDir = pos.DirectionTo(lookAt);
        var aligned = lookDir.Slide(up).Normalized();
        var right = up.Cross(aligned);
        var fwd = right.Cross(up);
        var basis = new Basis(right, up, -fwd);
        return new Transform3D(basis, pos);
    }

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

    #region Altitude

    public static float Altitude(this Terrain terrain, in Vector3 pos) => pos.Y - terrain.GetHeight(pos.X, pos.Z);
    public static float Altitude(this Terrain terrain, in Vector3 pos, int digits) => terrain.Altitude(pos).Rounded(digits);
    public static int AltitudeInt(this Terrain terrain, in Vector3 pos) => (int)terrain.Altitude(pos, 0);
    public static float AltitudeDbg(this Terrain terrain, in Vector3 pos) => terrain.Altitude(pos, 3);

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

    private static float DistanceTo(this Terrain terrain, in Vector3 pos)
        => pos.Y - terrain.GetHeight(pos.X, pos.Z);

    public static float MinDistanceTo(this Terrain terrain, in Vector3 pos, float radius) => terrain.DistanceTo(pos, radius, HeightType.Min);
    public static float MaxDistanceTo(this Terrain terrain, in Vector3 pos, float radius) => terrain.DistanceTo(pos, radius, HeightType.Max);
    public static float AvgDistanceTo(this Terrain terrain, in Vector3 pos, float radius) => terrain.DistanceTo(pos, radius, HeightType.Avg);
    private static float DistanceTo(this Terrain terrain, in Vector3 pos, float radius, HeightType t)
        => pos.Y - terrain.GetHeight(pos.X, pos.Z, radius, t);

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
