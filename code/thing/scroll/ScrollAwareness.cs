using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollAwareness : Scroll
{
    public ScrollAwareness()
	{
        ScrollType = ScrollType.Awareness;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Become aware of the level layout";
        Tooltip = "A scroll of Awareness";

        SetTattoo(Globals.Icon(IconType.Awareness));
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😏", (int)PlayerIconPriority.UseScroll, 1.0f);

        RevealLevel(user);

        Destroy();
    }

    public static void RevealLevel(Thing user)
    {
        user.AddFloater(Globals.Icon(IconType.Awareness), 1.1f, new Vector2(0, -3f), new Vector2(0, -15f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.2f, opacity: 0.8f);

        // todo: if AI uses, learns about all target positions

        if (user.Brain is RoguemojiPlayer player)
        {
            player.RevealEntireLevelClient();
            player.VfxFlashCamera(0.3f, new Color(0.425f, 0.625f, 0.825f));
        }
    }
}
