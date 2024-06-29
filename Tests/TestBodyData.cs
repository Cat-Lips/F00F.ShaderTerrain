using System;
using System.Linq;
using Godot;

namespace F00F.ShaderTerrain.Tests
{
    public enum ShapeType
    {
        Cube,
        Sphere,
        Capsule,
        Cylinder,
        Random,
    }

    public partial class TestBodyData : Resource
    {
        private static readonly ShapeType[] ShapeTypes = Enum.GetValues<ShapeType>().Except([ShapeType.Random]).ToArray();

        private float _shapeSize = 1;
        private ShapeType _shapeType = ShapeType.Sphere;

        public float ShapeSize { get => _shapeSize; set => this.Set(ref _shapeSize, Math.Clamp(value, 0, 10)); }
        public ShapeType ShapeType { get => _shapeType; set => this.Set(ref _shapeType, value); }
        public ShaderTerrain.ShapeType ColliderType { get => TerrainCollider.ShapeType; set => this.Set(ref TerrainCollider.ShapeType, value); }

        internal ShapeType GetShape()
            => ShapeType is ShapeType.Random ? ShapeTypes.GetRandom() : ShapeType;
    }
}
