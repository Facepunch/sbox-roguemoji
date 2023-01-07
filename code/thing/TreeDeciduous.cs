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
        Description = "A tall tree.";
        Tooltip = "A deciduous tree.";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Solid | ThingFlags.Selectable;
		PathfindMovementCost = 999f;
		SightBlockAmount = 13;

        if (Game.IsClient)
        {
            WieldedThingOffset = new Vector2(8.6f, 6.2f);
            WieldedThingFontSize = 14;
            InfoWieldedThingOffset = new Vector2(16f, 19f);
            InfoWieldedThingFontSize = 26;
        }

        //SetTattoo("🍎", scale: 0.4f, offset: new Vector2(0f, 0f), offsetWielded: Vector2.Zero, offsetInfo: new Vector2(0f, 0f), offsetCharWielded: Vector2.Zero, offsetInfoWielded: Vector2.Zero);
    }

    public override void OnSpawned()
    {
        base.OnSpawned();

        if(Game.Random.Float(0f, 1f) < 0.5f)
        {
            var nut = RoguemojiGame.Instance.SpawnThing<Nut>(CurrentLevelId);
            WieldAndRemoveFromGrid(nut);
        }
    }

    public override void OnBumpedIntoBy(Thing thing)
    {
        base.OnBumpedIntoBy(thing);

        if(WieldedThing != null)
        {
            if(ContainingGridManager.GetRandomEmptyAdjacentGridPos(GridPos, out var dropGridPos, allowNonSolid: true))
            {
                var droppedThing = WieldedThing;

                ContainingGridManager.AddThing(droppedThing);
                droppedThing.SetGridPos(dropGridPos);
                droppedThing.VfxFly(GridPos, 0.2f);

                WieldThing(null);
            }
        }
    }
}
