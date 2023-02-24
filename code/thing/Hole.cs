using Sandbox;
using System;

namespace Roguemoji;
public partial class Hole : Thing
{
	public Hole()
	{
        DisplayIcon = "️🕳";
        DisplayName = "Hole";
        Description = "A deep hole leading to another area";
        Tooltip = "A hole";
        IconDepth = (int)IconDepthLevel.Hole;
        Flags = ThingFlags.Exclusive | ThingFlags.Selectable | ThingFlags.CantBePushed;
        PathfindMovementCost = 15f;
        Flammability = 0;

        if (Game.IsClient)
        {
            CharSkip = 1;
        }
    }

    public override void OnMovedOntoBy(Thing thing, IntVector fromGridPos)
    {
        base.OnMovedOntoBy(thing, fromGridPos);

        if (thing.Brain is RoguemojiPlayer player)
            SwallowThing(thing);
    }

    public override void OnMovedOntoThing(Thing thing, IntVector fromGridPos)
    {
        base.OnMovedOntoThing(thing, fromGridPos);

        if (thing.Brain is RoguemojiPlayer player)
            SwallowThing(thing);
    }

    void SwallowThing(Thing thing)
    {
        thing.RemoveComponent<VfxSlide>();

        var nextLevelId = (CurrentLevelId == LevelId.Forest1) ? LevelId.Forest2 : LevelId.Forest3;

        var exitingLevel = thing.AddComponent<CExitingLevel>();
        exitingLevel.TargetLevelId = nextLevelId;
    }
}
