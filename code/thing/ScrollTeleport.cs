using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollTeleport : Thing
{
    [Net] public int ReqInt { get; private set; }

    public override string ChatDisplayIcons => $"📜{Globals.Icon(IconType.Teleport)}";
    public override string AbilityName => "Read Scroll";

    public ScrollTeleport()
	{
		DisplayIcon = "📜";
        DisplayName = "Scroll of Teleport";
        Description = "Teleport to a random place on the current floor";
        Tooltip = "A scroll of Teleport";
        IconDepth = 0;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        SetTattoo(Globals.Icon(IconType.Teleport), scale: 0.5f, offset: new Vector2(1f, 0.5f), offsetWielded: new Vector2(0f, 0.3f), offsetInfo: new Vector2(8f, 5f), offsetCharWielded: new Vector2(2f, 0.5f), offsetInfoWielded: new Vector2(3f, 2f));

        if (Game.IsServer)
        {
            ReqInt = 1;
            AddTrait(AbilityName, "🔥", "Sacrifice to cast the inscribed spell", offset: new Vector2(0f, -2f), tattooIcon: "📜", tattooScale: 0.45f, tattooOffset: new Vector2(0f, 4f));
            AddTrait("", GetStatIcon(StatType.Intelligence), Globals.GetStatReqString(StatType.Intelligence, ReqInt, VerbType.Read), offset: new Vector2(0f, -1f), labelText: $"≥{ReqInt}", labelFontSize: 16, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        var intelligence = user.GetStatClamped(StatType.Intelligence);
        if (intelligence < ReqInt)
        {
            if (shouldLogMessage && user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {ReqInt}{GetStatIcon(StatType.Intelligence)}  to use {ChatDisplayIcons} but you only have {intelligence}{GetStatIcon(StatType.Intelligence)}");

            return false;
        }

        return true;
    }

    public override void Use(Thing user)
    {
        if(user.ContainingGridManager.GetRandomEmptyGridPos(out var targetGridPos, allowNonSolid: true))
        {
            RoguemojiGame.Instance.AddFloater("✨", user.GridPos, 0.8f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), "", requireSight: true, EasingType.SineOut, fadeInTime: 0.2f);
            RoguemojiGame.Instance.AddFloater("✨", targetGridPos, 0.5f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), "", requireSight: true, EasingType.SineOut, fadeInTime: 0.1f);

            user.SetGridPos(targetGridPos);

            if (user is RoguemojiPlayer player)
                player.RecenterCamera();

            Destroy();

            base.Use(user);
        }
    }
}
