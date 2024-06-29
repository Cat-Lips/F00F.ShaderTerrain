namespace F00F;

public partial class TerrainConfig
{
    private void InitNoise()
    {
        Noise.Shader = Shader;
        Noise.ShaderMaterial = ShaderMaterial;

        Noise.SetShaderDef(ShaderName.UseBlending, UseBlending);
        Noise.SetShaderDef(ShaderName.UseGradient, UseGradient);
        Noise.SetShaderDef(ShaderName.RegionCount, Regions?.Length ?? 1);

        OnShapeChanged.Run();
        Noise.Changed -= OnShapeChanged.Run;
        Noise.Changed += OnShapeChanged.Run;
    }
}
