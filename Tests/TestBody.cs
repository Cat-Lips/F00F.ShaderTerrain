using System;
using System.Diagnostics;
using Godot;

namespace F00F.ShaderTerrain.Tests
{
    public partial class TestBody : RigidBody3D
    {
        private static readonly PackedScene _scene = Utils.LoadScene<TestBody>();
        public static TestBody Instantiate() { var x = _scene.Instantiate<TestBody>(); x.Initialise(); return x; }

        public static int DropCount { get; private set; }
        public static int ShapeCount { get; private set; }
        public static TestBodyData Config { get; } = new();

        static TestBody()
        {
            TerrainCollider.Falling += x =>
            {
                Debug.Assert(x is TestBody);
                x.QueueFree();
                ++DropCount;
            };
        }

        private void Initialise()
        {
            InitShape();
            InitPhysics();
            InitShapeCount();

            void InitShape()
            {
                var mesh = GetNode<MeshInstance3D>("Mesh");
                var shape = GetNode<CollisionShape3D>("Shape");

                var myShape = Config.GetShape();
                mesh.Mesh = NewMesh(myShape);
                shape.Shape = NewShape(myShape);
                mesh.Scale *= Config.ShapeSize;
                shape.Scale *= Config.ShapeSize;

                static Mesh NewMesh(ShapeType type) => type switch
                {
                    ShapeType.Cube => new BoxMesh(),
                    ShapeType.Sphere => new SphereMesh(),
                    ShapeType.Capsule => new CapsuleMesh(),
                    ShapeType.Cylinder => new CylinderMesh(),
                    _ => throw new NotImplementedException(),
                };

                static Shape3D NewShape(ShapeType type) => type switch
                {
                    ShapeType.Cube => new BoxShape3D(),
                    ShapeType.Sphere => new SphereShape3D(),
                    ShapeType.Capsule => new CapsuleShape3D(),
                    ShapeType.Cylinder => new CylinderShape3D(),
                    _ => throw new NotImplementedException(),
                };
            }

            void InitPhysics()
            {
                Mass = Config.ShapeSize * 100;
                PhysicsMaterialOverride.Bounce = Random.Shared.NextSingle();
                PhysicsMaterialOverride.Friction = Random.Shared.NextSingle();
            }

            void InitShapeCount()
            {
                TreeEntered += () => ++ShapeCount;
                TreeExiting += () => --ShapeCount;
            }
        }
    }
}
