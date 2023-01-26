using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollTeleport : Scroll
{
    public override string AbilityName => "Read Scroll";

    public ScrollTeleport()
	{
        ScrollType = ScrollType.Teleport;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Teleport to a random place on the current floor";
        Tooltip = "A scroll of Teleport";

        SetTattoo(Globals.Icon(IconType.Teleport));

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "🔥", $"Sacrifice to cast the inscribed spell", offset: new Vector2(0f, -2f), tattooIcon: "📜", tattooScale: 0.45f, tattooOffset: new Vector2(0f, 4f), isAbility: true);
        }
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        TeleportThing(user);
        RoguemojiGame.Instance.AddFloater("✨", user.GridPos, 0.8f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.2f);

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();
    }

    public static void TeleportThing(Thing thing)
    {
        if (thing.ContainingGridManager.GetRandomEmptyGridPos(out var targetGridPos, allowNonSolid: true))
        {
            RoguemojiGame.Instance.AddFloater("✨", targetGridPos, 1.0f, thing.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);

            thing.SetGridPos(targetGridPos);

            if (thing is RoguemojiPlayer player)
                player.RecenterCamera();
        }
        else
        {
            RoguemojiGame.Instance.AddFloater("✨", thing.GridPos, 0.8f, thing.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.2f);
        }
    }
}
