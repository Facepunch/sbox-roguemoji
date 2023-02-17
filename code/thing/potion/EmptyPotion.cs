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
            thing.Destroy();
            Destroy();

            //user.ContainingGridManager.AddFloater("✨", user.GridPos, 0.8f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.2f);
            //user.ContainingGridManager.AddFloater("✨", targetGridPos, 0.5f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);
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
