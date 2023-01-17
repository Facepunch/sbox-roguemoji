using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum ScrollType { Blink, Teleport, Fear, Telekinesis }
public partial class Scroll : Thing
{
    [Net] public ScrollType ScrollType { get; protected set; }
    public override string ChatDisplayIcons => GetChatDisplayIcons(ScrollType);

    public Scroll()
    {
        DisplayIcon = "📜";
        IconDepth = 0;
    }

    public static string GetDisplayName(ScrollType scrollType)
    {
        switch (scrollType)
        {
            case ScrollType.Blink: return "Scroll of Blink";
            case ScrollType.Fear: return "Scroll of Fear";
            case ScrollType.Telekinesis: return "Scroll of Telekinesis";
            case ScrollType.Teleport: return "Scroll of Teleport";
        }

        return "";
    }

    public static string GetChatDisplayIcons(ScrollType scrollType)
    {
        switch (scrollType)
        {
            case ScrollType.Blink: return $"📜{Globals.Icon(IconType.Blink)}";
            case ScrollType.Fear: return $"📜{Globals.Icon(IconType.Fear)}";
            case ScrollType.Telekinesis: return $"📜{Globals.Icon(IconType.Telekinesis)}";
            case ScrollType.Teleport: return $"📜{Globals.Icon(IconType.Teleport)}";
        }

        return "🧉";
    }

    public override void Use(Thing user)
    {
        base.Use(user);
        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);
    }

    public override void Use(Thing user, Direction direction)
    {
        base.Use(user, direction);
        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);
    }

    public override void Use(Thing user, IntVector targetGridPos)
    {
        base.Use(user, targetGridPos);
        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);
    }

    public void SetTattoo(string icon)
    {
        SetTattoo(icon, scale: 0.5f, offset: new Vector2(1.5f, 0f), offsetWielded: new Vector2(0.5f, 2.3f), offsetInfo: new Vector2(8f, 5f), offsetCharWielded: new Vector2(0.5f, -2.5f), offsetInfoWielded: new Vector2(2.5f, 1f));
    }
}
