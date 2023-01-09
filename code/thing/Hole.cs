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
        IconDepth = 1;
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

        if (thing is RoguemojiPlayer player)
        {
            player.RemoveComponent<VfxSlide>();

            var nextLevelId = CurrentLevelId == LevelId.Forest0 ? LevelId.Forest1 : LevelId.Forest2;
            //RoguemojiGame.Instance.ChangePlayerLevel(player, nextLevelId, shouldAnimateFall: true);

            var exitingLevel = player.AddComponent<CExitingLevel>();
            exitingLevel.TargetLevelId = nextLevelId;

            //player.VfxFadeCamera(lifetime: 0.5f, shouldFadeOut: false);
        }
    }
}
