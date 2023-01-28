using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollConfetti : Scroll
{
    public ScrollConfetti()
	{
        ScrollType = ScrollType.Confetti;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Tosses all your items in the air";
        Tooltip = "A scroll of Confetti";

        SetTattoo(Globals.Icon(IconType.Confetti));
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        Destroy();

        if(user is RoguemojiPlayer player)
            user.AddComponent<CConfetti>();

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();
    }
}
