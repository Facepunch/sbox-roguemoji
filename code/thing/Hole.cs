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
        Flags = ThingFlags.Exclusive | ThingFlags.Selectable;
        PathfindMovementCost = 15f;

        if (Game.IsClient)
        {
            CharSkip = 1;
        }
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);

        if (thing.Brain is RoguemojiPlayer player)
            SwallowThing(thing);
    }

    public override void OnMovedOntoThing(Thing thing)
    {
        base.OnMovedOntoThing(thing);

        if (thing.Brain is RoguemojiPlayer player)
            SwallowThing(thing);
    }

    void SwallowThing(Thing thing)
    {
        thing.RemoveComponent<VfxSlide>();

        var nextLevelId = (CurrentLevelId == LevelId.Forest0) ? LevelId.Forest1 : LevelId.Forest2;

        var exitingLevel = thing.AddComponent<CExitingLevel>();
        exitingLevel.TargetLevelId = nextLevelId;
    }
}
