using Sandbox;
using System;
using System.Linq;

namespace Roguemoji;
public partial class TreeDeciduous : Thing
{
    public bool HasDroppedLeaf { get; private set; }

    public int HealthAmount { get; set; }

	public TreeDeciduous()
	{
		DisplayIcon = "🌳";
        DisplayName = "Tree";
        Description = "A tall deciduous tree";
        Tooltip = "A tree";
        IconDepth = (int)IconDepthLevel.Solid;
        Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanWieldThings;
		PathfindMovementCost = 999f;
        HealthAmount = 400;

        if (Game.IsServer)
        {
            InitStat(StatType.SightBlockAmount, 13);
            InitStat(StatType.Health, HealthAmount, min: 0, max: HealthAmount);
        }
        else 
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

        Log.Info($"tree - OnBumpedIntoBy - WieldedThing: {WieldedThing}");

        if (WieldedThing != null)// && thing != WieldedThing)
        {
            TryDropThingNearby(WieldedThing);
        }
        else
        {
            if (!HasDroppedLeaf && Game.Random.Int(0, 4) == 0)
            {
                if (ContainingGridManager.GetRandomEmptyAdjacentGridPos(GridPos, out var dropGridPos, allowNonSolid: true))
                {
                    var leaf = ContainingGridManager.SpawnThing<Leaf>(dropGridPos);
                    leaf.SetIcon("🍃");

                    leaf.VfxFly(GridPos, lifetime: 0.25f, heightY: 35f, progressEasingType: EasingType.Linear, heightEasingType: EasingType.SineInOut);

                    leaf.CanBeSeenByPlayerClient(GridPos);

                    var tempIconDepth = leaf.AddComponent<CTempIconDepth>();
                    tempIconDepth.Lifetime = 0.35f;
                    tempIconDepth.SetTempIconDepth((int)IconDepthLevel.Projectile);

                    HasDroppedLeaf = true;
                }
            }
        }
    }
}
