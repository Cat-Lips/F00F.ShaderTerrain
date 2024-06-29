using System;
using Godot;

namespace F00F;

using Camera = Godot.Camera3D;

[Tool]
public partial class Terrain : Node
{
    #region Private

    private Node Body => field ??= GetNode<Node>("Body");
    private Node3D Mesh => field ??= GetNode<Node3D>("Mesh");

    #endregion

    public event Action ConfigSet;

    public Action<Node3D> CustomFallThruAction { get; set; }

    public bool EnableBody { get; set; } = true;
    public bool ShowMesh { get => Mesh.Visible; set => Mesh.Visible = value; }

    #region Export

    [Export] public TerrainConfig Config { get; set => this.Set(ref field, value ?? new(), InitConfig, ConfigSet); }
    [Export] public Camera Camera { get; set => this.Set(ref field, value ?? GetCamera(), InitCamera); }

    #endregion

    public float GetHeight(float x, float z)
        => Config.GetHeight(x, z);

    public float GetHeight(in Vector3 pos)
        => GetHeight(pos.X, pos.Z);

    public float GetHeight(in Vector2 xz)
        => GetHeight(xz.X, xz.Y);

    #region Godot

    public Terrain()
    {
        if (!Editor.IsEditor)
            InitActors();
    }

    public sealed override void _Ready()
    {
        Config ??= new();
        Camera ??= GetCamera();
    }

    public sealed override void _Process(double _)
        => UpdateChunks();

    public sealed override void _PhysicsProcess(double _)
        => UpdateShapes();

    #endregion

    #region Private

    private Camera GetCamera()
        => GetViewport().GetCamera3D();

    #endregion
}
