using Sandbox;
using System;
using System.Linq;

namespace Roguemoji;
public partial class TreeDeciduous : Thing
{
	public TreeDeciduous()
	{
		DisplayIcon = "🌳";
        DisplayName = "Deciduous Tree";
        Description = "A tall tree";
        Tooltip = "A deciduous tree";
        IconDepth = 1;
		Flags = ThingFlags.Solid | ThingFlags.Selectable;
		PathfindMovementCost = 999f;
		SightBlockAmount = 13;

        if (Game.IsClient)
        {
            WieldedThingOffset = new Vector2(9.6f, 7.2f);
            WieldedThingFontSize = 14;
            InfoWieldedThingOffset = new Vector2(16f, 19f);
            InfoWieldedThingFontSize = 26;
        }
    }

    public override void OnSpawned()
    {
        base.OnSpawned();

        if(Game.Random.Float(0f, 1f) < 0.33f)
        {
            int randItemNum = Game.Random.Int(0, 11);
            Thing item = null;

            switch(randItemNum)
            {
                case 0: case 1: case 2: item = RoguemojiGame.Instance.SpawnThing<AppleRed>(CurrentLevelId); break;
                case 3: case 4: item = RoguemojiGame.Instance.SpawnThing<AppleGreen>(CurrentLevelId); break;
                case 5: case 6: item = RoguemojiGame.Instance.SpawnThing<Nut>(CurrentLevelId); break;
                case 7: item = RoguemojiGame.Instance.SpawnThing<Peach>(CurrentLevelId); break;
                case 8: item = RoguemojiGame.Instance.SpawnThing<Pear>(CurrentLevelId); break;
                case 9: item = RoguemojiGame.Instance.SpawnThing<Cherry>(CurrentLevelId); break;
                case 10: item = RoguemojiGame.Instance.SpawnThing<Orange>(CurrentLevelId); break;
                case 11: item = RoguemojiGame.Instance.SpawnThing<Lemon>(CurrentLevelId); break;
            }

            WieldAndRemoveFromGrid(item);
        }
    }

    public override void OnBumpedIntoBy(Thing thing)
    {
        base.OnBumpedIntoBy(thing);

        if(WieldedThing != null && Game.Random.Int(0, 1) == 0)
        {
            if (ContainingGridManager.GetRandomEmptyAdjacentGridPos(GridPos, out var dropGridPos, allowNonSolid: true))
            {
                var droppedThing = WieldedThing;

                ContainingGridManager.AddThing(droppedThing);
                droppedThing.SetGridPos(dropGridPos);
                droppedThing.VfxFly(GridPos, lifetime: 0.2f, heightY: 30f, progressEasingType: EasingType.Linear, heightEasingType: EasingType.SineInOut);

                WieldThing(null);
            }
        }
    }
}
