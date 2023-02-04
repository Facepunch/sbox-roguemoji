using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollIdentify : Scroll
{
    public override GridType AimingGridType => GridType.Inventory;

    public ScrollIdentify()
	{
        ScrollType = ScrollType.Identify;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Reveals unidentified scrolls or potions";
        Tooltip = "A scroll of Identify";

        SetTattoo(Globals.Icon(IconType.Identify));
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        var player = user.Brain as RoguemojiPlayer;
        if (player == null)
            return;

        var item = player.InventoryGridManager.GetThingsAt(targetGridPos).FirstOrDefault();
        if (item == null)
            return;

        if (item is Scroll scroll && !player.IsScrollTypeIdentified(scroll.ScrollType))
        {
            RoguemojiGame.Instance.RevealScroll(scroll.ScrollType, user.GridPos, user.CurrentLevelId);
            scroll.AddFloater(Globals.Icon(IconType.Identified), 0.6f, new Vector2(0f, 0f), new Vector2(0, -10f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.1f, scale: 0.8f, opacity: 1f);
        }
        else if(item is Potion potion && !player.IsPotionTypeIdentified(potion.PotionType))
        {
            RoguemojiGame.Instance.RevealPotion(potion.PotionType, user.GridPos, user.CurrentLevelId);
            potion.AddFloater(Globals.Icon(IconType.Identified), 0.6f, new Vector2(0f, 0f), new Vector2(0, -10f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.1f, scale: 0.8f, opacity: 1f);
        }

        Destroy();
    }

    public override HashSet<IntVector> GetAimingTargetCellsClient() 
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        return Scroll.GetInventoryAimingCells(ThingWieldingThis);
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        return Scroll.IsPotentialInventoryAimingCell(gridPos, ThingWieldingThis);
    }
}
