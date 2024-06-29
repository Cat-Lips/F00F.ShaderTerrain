# F00F.ShaderTerrain
Godot 4 C# Shader Terrain

Heavily based on [Ditzy Ninja](https://www.youtube.com/@ditzyninja)'s most excellent tutorials:
 - [Massive Infinite Terrain: Clipmap & Collisions](https://www.youtube.com/watch?v=Hgv9iAdazKg)
 - [Massive Infinite Terrain: LOD & Mesh Stitching](https://www.youtube.com/watch?v=jDM0m4WuBAg)
 - [Calculating normals on the GPU](https://www.youtube.com/watch?v=izsMr5Pyk2g)

 With elements incorporated from [Sebastian Lague](https://www.youtube.com/@SebastianLague)'s most excellent series:
  - [Procedural Terrain Generation](https://www.youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3)

Both highly recommended if interested in exploring this area

## TODO:
 - Scale is broken, will fix later
 - When using the height curve, collision mesh doesn't quite line up with terrain, probably something to do with height curve image conversion when being passed to shader
 - Better drop handling (ie, when objects drop through collision mesh due to fast movement or rapid terrain changes)
 - Water, foliage, grass, etc, scattered based on region type