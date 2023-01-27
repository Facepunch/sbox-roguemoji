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

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "🔥", $"Sacrifice to cast the inscribed spell", offset: new Vector2(0f, -2f), tattooIcon: "📜", tattooScale: 0.45f, tattooOffset: new Vector2(0f, 4f), isAbility: true);
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        var player = user as RoguemojiPlayer;
        if (player == null)
            return;

        var item = player.InventoryGridManager.GetThingsAt(targetGridPos).FirstOrDefault();
        if (item == null)
            return;

        RoguemojiGame.Instance.AddFloaterInventory(player, Globals.Icon(IconType.Identified), item.GridPos, 1f, new Vector2(0f, 0f), new Vector2(0, -10f), height: 0f, text: "", EasingType.QuadOut, fadeInTime: 0.5f, scale: 0.8f, opacity: 0.66f, parent: item);

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

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
