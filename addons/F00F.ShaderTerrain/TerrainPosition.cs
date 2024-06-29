using Godot;

namespace F00F.ShaderTerrain
{
    public class TerrainPosition : GlobalShaderParam<TerrainPosition, Vector2>
    {
        protected override string ShaderType => "vec2";
        protected override string ParamName => "terrain_pos";
    }
}
