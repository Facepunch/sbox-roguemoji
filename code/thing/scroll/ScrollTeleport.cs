using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollTeleport : Scroll
{
    public ScrollTeleport()
	{
        ScrollType = ScrollType.Teleport;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Teleport to a random place on the current floor";
        Tooltip = "A scroll of Teleport";

        SetTattoo(Globals.Icon(IconType.Teleport));
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        TeleportThing(user);
        user.ContainingGridManager.AddFloater("✨", user.GridPos, 0.8f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.2f);

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();
    }

    public static void TeleportThing(Thing thing)
    {
        if (thing.ContainingGridManager.GetRandomEmptyGridPos(out var targetGridPos, allowNonSolid: true))
        {
            thing.ContainingGridManager.AddFloater("✨", targetGridPos, 1.0f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);

            thing.SetGridPos(targetGridPos);

            if (thing.Brain is RoguemojiPlayer player)
                player.RecenterCamera();
        }
    }
}
