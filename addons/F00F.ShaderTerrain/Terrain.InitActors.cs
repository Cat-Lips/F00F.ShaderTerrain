using Godot;

namespace F00F;

public partial class Terrain
{
    private void InitActors()
    {
        ChildEnteredTree += OnChildEntered;
        ChildExitingTree += OnChildExiting;

        void OnChildEntered(Node node)
        {
            if (node is CollisionObject3D actor)
                Actors.Add(actor);
        }

        void OnChildExiting(Node node)
        {
            if (node is CollisionObject3D actor)
                Actors.Remove(actor);
        }
    }
}
