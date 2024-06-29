using System;
using Godot;

namespace F00F.ShaderTerrain
{
    public enum ShapeType { Polygon, HeightMap }
    internal partial class TerrainCollider : CollisionShape3D
    {
        public static ShapeType ShapeType = ShapeType.HeightMap;

        // Fast moving objects can fall through terrain
        // By default, falling bodies are deleted
        // Connect to event to provide alternative behaviour
        //   (eg, delete, respawn, adjust velocity/recalculate physics)
        public static event Action<PhysicsBody3D> Falling;

        #region Instantiate

        private static PackedScene _scene;
        private static PackedScene Scene => _scene ??= Utils.LoadScene<TerrainCollider>();

        public static TerrainCollider Instantiate(Terrain terrain, PhysicsBody3D source)
        {
            var scene = Scene.Instantiate<TerrainCollider>();
            scene.Initialise(terrain, source);
            return scene;
        }

        #endregion

        internal bool Reset = true;
        private Action ProcessPhysics;

        private void Initialise(Terrain terrain, PhysicsBody3D source)
        {
            Disabled = true;
            Name = source.Name;
            var bb = source.GetAabb();
            var size = Mathf.CeilToInt(bb.GetLongestAxisSize());

            InitShape();

            void InitShape()
            {
                var minHeight = float.MaxValue;
                var maxHeight = float.MinValue;
                Shape = MyShape.Create(ShapeType, size, GetHeight, out var UpdateShapeData, out var ApplyShapeData);
                this.ProcessPhysics = ProcessPhysics;

                float GetHeight(float x, float z)
                {
                    var height = terrain.GetHeight(x, z);
                    if (height < minHeight) minHeight = height;
                    if (height > maxHeight) maxHeight = height;
                    return height;
                }

                void ProcessPhysics()
                {
                    var sourcePos = source.Position;
                    var shapePos = sourcePos.RoundXZ();

                    UpdateShape();
                    EnableShape();

                    void UpdateShape()
                    {
                        if (Reset || Position != shapePos)
                        {
                            Reset = false;
                            Position = shapePos;
                            UpdateShapeData(shapePos);
                        }
                    }

                    void EnableShape()
                    {
                        var active = IsActive();
                        var falling = IsFalling();

                        Disabled = !active;

                        ApplyShapeData(active);
                        NotifyListeners(falling);

                        bool IsActive()
                        {
                            return IsBelowUpperLimit() && IsAboveLowerLimit();

                            bool IsBelowUpperLimit() => sourcePos.Y <= maxHeight + size * 2;
                            bool IsAboveLowerLimit() => sourcePos.Y >= minHeight - size * 2;
                        }

                        bool IsFalling()
                        {
                            return IsBelowLowerLimit();

                            bool IsBelowLowerLimit() => sourcePos.Y <= minHeight - size * 2;
                        }

                        void NotifyListeners(bool falling)
                        {
                            if (falling)
                            {
                                if (Falling is null)
                                    source.GetParent().RemoveChild(source, free: true);
                                else
                                    Falling?.Invoke(source);
                            }
                        }
                    }
                }
            }
        }

        #region Godot

        public override void _PhysicsProcess(double delta)
            => ProcessPhysics();

        #endregion

        private static class MyShape
        {
            public static Shape3D Create(ShapeType type, int size, Func<float, float, float> GetHeight, out Action<Vector3> Update, out Action<bool> Apply)
            {
                return type switch
                {
                    ShapeType.Polygon => NewPolygonShape(out Update, out Apply),
                    ShapeType.HeightMap => NewHeightMapShape(out Update, out Apply),
                    _ => throw new NotImplementedException(),
                };

                Shape3D NewPolygonShape(out Action<Vector3> Update, out Action<bool> Apply)
                {
                    var shape = new ConcavePolygonShape3D();
                    var data = CreateVertices();
                    Update = UpdateData;
                    Apply = ApplyData;
                    return shape;

                    Vector3[] CreateVertices() => new PlaneMesh
                    {
                        Size = Vector2.One * size,
                        SubdivideDepth = size - 1,
                        SubdivideWidth = size - 1,
                    }.GetFaces();

                    void UpdateData(Vector3 shapePos)
                    {
                        for (var i = 0; i < data.Length; ++i)
                        {
                            var hPos = shapePos + data[i];
                            data[i].Y = GetHeight(hPos.X, hPos.Z);
                        }
                    }

                    void ApplyData(bool active)
                        => shape.Data = active ? data : null;
                }

                Shape3D NewHeightMapShape(out Action<Vector3> Update, out Action<bool> Apply)
                {
                    var mapSize = size + 1;
                    var halfSize = size * .5f;

                    var shape = new HeightMapShape3D { MapWidth = mapSize, MapDepth = mapSize };
                    var data = shape.MapData;
                    Update = UpdateData;
                    Apply = ApplyData;
                    return shape;

                    void UpdateData(Vector3 shapePos)
                    {
                        var x0 = shapePos.X - halfSize;
                        var z0 = shapePos.Z - halfSize;
                        for (var x = 0; x < mapSize; ++x)
                        {
                            for (var z = 0; z < mapSize; ++z)
                            {
                                data[x + z * mapSize] = GetHeight(x0 + x, z0 + z);
                            }
                        }
                    }

                    void ApplyData(bool active)
                        => shape.MapData = active ? data : null;
                }
            }
        }
    }
}
