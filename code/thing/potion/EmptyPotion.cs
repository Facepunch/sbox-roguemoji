using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class EmptyPotion : Thing
{
    public override string AbilityName => "Fill Potion";

    public EmptyPotion()
    {
        DisplayIcon = "🧉";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;
        DisplayName = "Empty Potion";
        Description = "Can be filled with liquids";
        Tooltip = "An empty potion";
        Flammability = 13;

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "🚰", $"Fill potion with nearby puddle", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.6f, tattooOffset: new Vector2(-4f, 8f), isAbility: true);
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        var thing = user.ContainingGridManager.GetThingsAt(targetGridPos).WithAll(ThingFlags.Puddle).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        if(thing != null)
        {
            var puddle = thing as Puddle;
            if(puddle != null)
            {
                Thing newPotion = null;
                switch(puddle.LiquidType)
                {
                    case PotionType.Water: newPotion = ContainingGridManager.SpawnThing<PotionWater>(GridPos); break;
                    case PotionType.Blood: newPotion = ContainingGridManager.SpawnThing<PotionBlood>(GridPos); break;
                    case PotionType.Oil: newPotion = ContainingGridManager.SpawnThing<PotionOil>(GridPos); break;
                    case PotionType.Mud: newPotion = ContainingGridManager.SpawnThing<PotionMud>(GridPos); break;
                    case PotionType.Lava: newPotion = ContainingGridManager.SpawnThing<PotionLava>(GridPos); break;
                }

                if(newPotion != null)
                {
                    newPotion.ContainingGridManager.AddFloater("🚰", newPotion.GridPos, 1.0f, new Vector2(0, 2f), new Vector2(0, -9f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.SineOut, fadeInTime: 0.2f);
                    user.WieldThing(newPotion);
                }

                user.ContainingGridManager.AddFloater("🧉", targetGridPos, 0.5f, new Vector2(0, 3f), new Vector2(0, -8f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.SineOut, fadeInTime: 0.1f);

                puddle.Destroy();
                Destroy();
            }
        }
    }

    public override HashSet<IntVector> GetAimingTargetCellsClient()
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        int RADIUS = 1;
        return Scroll.GetArenaAimingCells(RADIUS, ThingWieldingThis);
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        int RADIUS = 1;
        return Scroll.IsPotentialArenaAimingCell(gridPos, RADIUS, ThingWieldingThis);
    }
}
