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
    }
}
