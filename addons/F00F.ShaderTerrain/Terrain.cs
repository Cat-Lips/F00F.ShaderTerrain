using System;
using Godot;

namespace F00F.ShaderTerrain
{
    [Tool]
    public partial class Terrain : Node
    {
        #region Export

        private TerrainData _config;
        [Export] public TerrainData Config { get => _config; set => this.Set(ref _config, value ?? TerrainData.Default(), OnConfigSet, ConfigSet); }
        public event Action ConfigSet;

        [Export] public Camera3D Camera { get; set; }

        #endregion

        public Node3D Mesh => (Node3D)GetNode("Mesh");
        public StaticBody3D Body => (StaticBody3D)GetNode("Body");

        public float GetHeight(float x, float z)
            => Config.GetHeight(x, z);

        #region Godot

        public override void _Ready()
        {
            Config ??= null;
            ChildEnteredTree += OnChildAdded;
            ChildExitingTree += OnChildRemoved;
            this.ForEachChild<PhysicsBody3D>(OnChildAdded);

            void OnChildAdded(Node child)
            {
                if (child is RigidBody3D pBody) AddCollider(pBody);
                else if (child is CharacterBody3D cBody) AddCollider(cBody);

                void AddCollider(PhysicsBody3D body)
                {
                    var collider = TerrainCollider.Instantiate(this, body);
                    body.SetMeta(nameof(TerrainCollider), collider);
                    Body.AddChild(collider, forceReadableName: true);
                }
            }

            void OnChildRemoved(Node child)
            {
                if (child is RigidBody3D pBody) RemoveCollider(pBody);
                else if (child is CharacterBody3D cBody) RemoveCollider(cBody);

                void RemoveCollider(PhysicsBody3D body)
                {
                    var collider = (TerrainCollider)body.GetMeta(nameof(TerrainCollider));
                    Body.RemoveChild(collider, free: true);
                }
            }
        }

        public override void _Process(double delta)
        {
            if (Camera is null) return;
            this.Clamp(Camera, Camera.Near);

            var terrainPos = Camera.Position.RoundXZ()
                .Snapped(Vector3.One * Config.ScaledChunkSize);

            Mesh.Position = terrainPos;
            //TerrainPosition.Instance.Value = terrainPos.XZ();
            Config.ShaderMaterial.SetShaderParameter("terrain_pos", terrainPos.XZ());
        }

        #endregion

        #region Private

        private void OnConfigSet()
        {
            ResetScale();
            ResetChunks();
            ResetShapes();

            Config.ScaleValueChanged.Action += ResetScale;
            Config.ChunkValueChanged.Action += ResetChunks;
            Config.HeightValueChanged.Action += ResetShapes;

            void ResetScale()
            {
                Mesh.Scale = Vector3.One * Config.ChunkScale;
                Body.Scale = Vector3.One * Config.ChunkScale;
            }

            void ResetChunks()
            {
                Mesh.RemoveChildren(free: true);
                foreach (var ((x, z), lod) in Utils.Spiral(Config.ChunkRadius))
                {
                    var chunk = TerrainChunk.Instantiate(Config, x, z, lod);
                    Mesh.AddChild(chunk, forceReadableName: true);
                }
            }

            void ResetShapes()
                => Body.ForEachChild<TerrainCollider>(x => x.Reset = true);
        }

        #endregion
    }
}
