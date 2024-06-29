using System.Collections.Generic;
using System.Linq;
using Godot;

namespace F00F;

public partial class TerrainConfig
{
    private void LoadAssets() => LoadAssets(false);
    private void LoadAssets(bool reset)
    {
        if (!FS.DirExists(Assets)) return;

        LoadConfig(out var cfg);
        LoadTextures(out var textures);
        CreateRegions(out var regions);
        AddFoliage();
        AddObjects();
        AddAudio();
        Apply();

        void LoadConfig(out ConfigFile cfg)
        {
            cfg = Config.LoadConfig();
            if (reset) cfg.Clear();
        }

        void LoadTextures(out Dictionary<string, Texture2D> textures)
        {
            textures = [];
            foreach (var file in FS.ListRes(Assets.PathJoin("Textures")))
            {
                var texture = GD.Load<Texture2D>(file);
                if (texture is null) continue;

                textures.TryAdd(GetNameFrom(file.GetFileBaseName()), texture);
            }

            //TryAddLocalTextures(ref textures);
            //TryAddNestedTextures(ref textures);

            //void TryAddLocalTextures(ref Dictionary<string, Texture2D> textures)
            //{
            //    foreach (var file in texturesRoot.GetFilesExcept("txt"))
            //    {
            //        GD.Print("Loading: ", file);
            //        var texture = GD.Load<Texture2D>(file);
            //        if (texture is null) continue;

            //        textures.TryAdd(GetNameFrom(file.GetFileBaseName()), texture);
            //    }
            //}

            //void TryAddNestedTextures(ref Dictionary<string, Texture2D> textures)
            //{
            //    foreach (var dir in texturesRoot.DirNames())
            //    {
            //        var textureDir = texturesRoot.PathJoin(dir);
            //        if (!TryGetTextures(textureDir, out var texture))
            //            continue;

            //        textures.TryAdd(GetNameFrom(dir), texture);
            //    }

            //    bool TryGetTextures(string dir, out Texture2D color/*, out Texture2D normal, out Texture2D roughness, out Texture2D displacement*/)
            //    {
            //        var files = dir.GetFilesWith("color"/*, "normalgl", "roughness", "displacement"*/);
            //        color = files.TryGetValue("color", out var file) ? GD.Load<Texture2D>(file) : null;
            //        //normal = files.TryGetValue("normalgl", out file) ? GD.Load<Texture2D>(file) : null;
            //        //roughness = files.TryGetValue("roughness", out file) ? GD.Load<Texture2D>(file) : null;
            //        //displacement = files.TryGetValue("displacement", out file) ? GD.Load<Texture2D>(file) : null;
            //        return color is not null;
            //    }
            //}

            static string GetNameFrom(string x)
                => x.Split('.', ' ', '-').Last();
        }

        void CreateRegions(out TerrainType[] regions)
        {
            regions = Regions().ToArray();

            IEnumerable<TerrainType> Regions()
            {
                var step = 1f / textures.Count;

                return textures
                    .OrderBy(cfg.GetSections())
                    .Select((x, i) => NewRegion(i, x.Key, x.Value));

                TerrainType NewRegion(int idx, string name, Texture2D texture) => new()
                {
                    Name = name,

                    Texture = texture,
                    TextureScale = cfg.GetV(name, nameof(TerrainType.TextureScale), () => TerrainType.Default.TextureScale),
                    BlendStrength = cfg.GetV(name, nameof(TerrainType.BlendStrength), () => TerrainType.Default.BlendStrength),

                    TextureTint = cfg.GetV(name, nameof(TerrainType.TextureTint), () => TerrainType.Utils.TextureTint(texture)),
                    TintStrength = cfg.GetV(name, nameof(TerrainType.TintStrength), () => TerrainType.Default.TintStrength),

                    MinSlope = cfg.GetV(name, nameof(TerrainType.MinSlope), () => TerrainType.Utils.MinSlope(idx, step)),
                    MaxSlope = cfg.GetV(name, nameof(TerrainType.MaxSlope), () => TerrainType.Utils.MaxSlope(idx, step)),
                    MinHeight = cfg.GetV(name, nameof(TerrainType.MinHeight), () => TerrainType.Utils.MinHeight(idx, step)),
                    MaxHeight = cfg.GetV(name, nameof(TerrainType.MaxHeight), () => TerrainType.Utils.MaxHeight(idx, step)),

                    Gradient = cfg.GetV(name, nameof(TerrainType.Gradient), () => TerrainType.Utils.Gradient(idx, step)),
                    LowerSmooth = cfg.GetV(name, nameof(TerrainType.LowerSmooth), () => TerrainType.Utils.LowerSmooth(idx, step)),
                    UpperSmooth = cfg.GetV(name, nameof(TerrainType.UpperSmooth), () => TerrainType.Utils.UpperSmooth(idx, step)),
                };
            }
        }

        void AddFoliage()
        {

        }

        void AddObjects()
        {

        }

        void AddAudio()
        {

        }

        void Apply()
            => Regions = regions;
    }
}
