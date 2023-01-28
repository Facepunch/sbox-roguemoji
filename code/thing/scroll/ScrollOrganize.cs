using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollOrganize : Scroll
{
    public ScrollOrganize()
	{
        ScrollType = ScrollType.Organize;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Organizes the items below your hotbar";
        Tooltip = "A scroll of Organize";

        SetTattoo(Globals.Icon(IconType.Organize));
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();

        var player = user as RoguemojiPlayer;
        if (player == null)
            return;

        player.AddComponent<COrganize>();

        //var orderedItems = player.InventoryGridManager.GetAllThings().Where(x => x.GridPos.y > 0).OrderBy(x => x.Tooltip);
        //int startingIndex = player.InventoryGridManager.GetIndex(new IntVector(0, 1));
        //for(int i = 0; i < orderedItems.Count(); i++)
        //{
        //    var thing = orderedItems.ElementAt(i);
        //    player.SwapGridThingPos(thing, GridType.Inventory, player.InventoryGridManager.GetGridPos(startingIndex + i));
        //}
    }
}
