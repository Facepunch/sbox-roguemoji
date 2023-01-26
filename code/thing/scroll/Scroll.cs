using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum ScrollType { Blink, Teleport, Fear, Telekinesis, Displace }
public partial class Scroll : Thing
{
    [Net] public ScrollType ScrollType { get; protected set; }
    public override string ChatDisplayIcons => GetChatDisplayIcons(ScrollType);

    public Scroll()
    {
        DisplayIcon = "📜";
        IconDepth = (int)IconDepthLevel.Normal;
    }

    public static string GetDisplayName(ScrollType scrollType)
    {
        switch (scrollType)
        {
            case ScrollType.Blink: return "Scroll of Blink";
            case ScrollType.Fear: return "Scroll of Fear";
            case ScrollType.Telekinesis: return "Scroll of Telekinesis";
            case ScrollType.Teleport: return "Scroll of Teleport";
            case ScrollType.Displace: return "Scroll of Displace";
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
            case ScrollType.Displace: return $"📜{Globals.Icon(IconType.Displace)}";
        }

        return "🧉";
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (user is RoguemojiPlayer player && player.IsConfused)
        {
            if (shouldLogMessage)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"{Globals.Icon(IconType.Confusion)}Too confused to read scrolls!");

            return false;
        }

        return true;
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
        SetTattoo(icon, scale: 0.5f, offset: new Vector2(1.5f, 0f), offsetWielded: new Vector2(0.5f, 2.3f), offsetInfo: new Vector2(8f, 5f), offsetCharWielded: new Vector2(2.5f, -0.5f), offsetInfoWielded: new Vector2(2.5f, 1f));
    }

    public static HashSet<IntVector> GetAimingCells(int radius, Thing thingWieldingThis)
    {
        HashSet<IntVector> aimingCells = new HashSet<IntVector>();

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int distance = Utils.GetDistance(x, y);
                if (distance > radius)
                    continue;

                var gridPos = thingWieldingThis.GridPos + new IntVector(x, y);
                //if (thingWieldingThis.ContainingGridManager.GetThingsAtClient(gridPos).WithAll(ThingFlags.Solid).ToList().Count > 0)
                //    continue;

                aimingCells.Add(gridPos);
            }
        }

        return aimingCells;
    }

    public static bool IsPotentialAimingCell(IntVector gridPos, int radius, Thing thingWieldingThis)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int distance = Utils.GetDistance(x, y);
                if (distance > radius)
                    continue;

                var currGridPos = thingWieldingThis.GridPos + new IntVector(x, y);
                if (gridPos.Equals(currGridPos))
                    return true;
            }
        }

        return false;
    }
}
