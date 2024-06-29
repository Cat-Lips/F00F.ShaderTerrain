using System.Collections.Generic;
using System.Linq;
using Godot;

namespace F00F;

using static F00F.TerrainConfig.Enum;
using Actors = HashSet<CollisionObject3D>;
using BodyChunks = Dictionary<Vector2, StaticBody3D>;
using ChunkId = Vector2;

public partial class Terrain
{
    private Actors Actors { get; } = [];
    private BodyChunks BodyChunks { get; } = [];

    private bool ShapesDirty { get; set; }
    private void UpdateShapes()
    {
        if (ShapesDirty)
        {
            Body.RemoveChildren();
            BodyChunks.Clear();
            ShapesDirty = false;
        }

        var chunksToRemove = new HashSet<ChunkId>(BodyChunks.Keys);
        var requiredChunks = Actors.SelectMany(Chunks).Distinct();
        requiredChunks.Where(IsNewChunk).ForEach(AddChunk);
        chunksToRemove.ForEach(RemoveChunk);

        IEnumerable<ChunkId> Chunks(CollisionObject3D actor)
        {
            ApplyFallThroughStrategy();
            return GetChunks();

            IEnumerable<ChunkId> GetChunks()
            {
                if (!EnableBody) yield break;

                var pos = actor.Position;
                var radius = actor.GetAabb().GetLongestAxisSize() * .5f;

                if (ColliderRequired())
                {
                    foreach (var (id, _) in Utils.Spiral(CenterChunk(), ChunkRadius()))
                        yield return id;
                }

                bool ColliderRequired()
                    => pos.Y - radius < Config.Amplitude;

                ChunkId CenterChunk()
                    => pos.XZ().Snapped(Config.ShapeSize) / Config.ShapeSize;

                int ChunkRadius()
                    => Mathf.CeilToInt(radius / Config.ShapeSize);
            }

            void ApplyFallThroughStrategy()
            {
                switch (Config.FallThruAction)
                {
                    case FallThruAction.None: break;
                    case FallThruAction.Clamp: Clamp(); break;
                    case FallThruAction.Delete: Delete(); break;
                    case FallThruAction.Custom: Custom(); break;
                }

                void Clamp()
                {
                    if (actor is RigidBody3D body)
                        this.Clamp(body);
                    else this.Clamp(actor);
                }

                void Delete()
                {
                    if (this.PosBelowHeight(actor.Position))
                        this.RemoveChild(actor, free: true);
                }

                void Custom()
                    => CustomFallThruAction?.Invoke(actor);
            }
        }

        bool IsNewChunk(/*in */ChunkId id)
            => !chunksToRemove.Remove(id);

        void AddChunk(/*in */ChunkId id)
        {
            var body = Config.CreateShape(id);
            Body.AddChild(body, own: true);
            BodyChunks.Add(id, body);
        }

        void RemoveChunk(ChunkId id)
        {
            BodyChunks.Remove(id, out var body);
            Body.RemoveChild(body, free: true);
        }
    }
}
