﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum ScrollType { Blink, Teleport, Fear, Telekinesis, Displace, Confetti, Identify, }
public partial class Scroll : Thing
{
    [Net] public ScrollType ScrollType { get; protected set; }
    public override string AbilityName => "Read Scroll";
    public override string ChatDisplayIcons => GetChatDisplayIcons(ScrollType);

    public Scroll()
    {
        DisplayIcon = "📜";
        IconDepth = (int)IconDepthLevel.Normal;

        if (Game.IsServer)
        {
            AddTrait(AbilityName, Globals.Icon(IconType.SacrificeScroll), $"Sacrifice to cast the inscribed spell", offset: new Vector2(0f, -2f), tattooIcon: "📜", tattooScale: 0.45f, tattooOffset: new Vector2(0f, 4f), isAbility: true);
        }
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
            case ScrollType.Confetti: return "Scroll of Confetti";
            case ScrollType.Identify: return "Scroll of Identify";
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
            case ScrollType.Confetti: return $"📜{Globals.Icon(IconType.Confetti)}";
            case ScrollType.Identify: return $"📜{Globals.Icon(IconType.Identify)}";
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
        UseResults(user);
    }

    public override void Use(Thing user, Direction direction)
    {
        base.Use(user, direction);
        UseResults(user);
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);
        UseResults(user);
    }

    void UseResults(Thing user)
    {
        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        if (user is RoguemojiPlayer player)
            RoguemojiGame.Instance.AddFloaterInventory(player, Globals.Icon(IconType.SacrificeScroll), GridPos, 0.3f, new Vector2(0f, 0f), new Vector2(0, 0f), height: 0f, text: "", EasingType.QuadOut, fadeInTime: 0.04f, scale: 0.9f, opacity: 0.8f, parent: null);
    }

    public void SetTattoo(string icon)
    {
        SetTattoo(icon, scale: 0.5f, offset: new Vector2(1.5f, 0f), offsetWielded: new Vector2(0.5f, 2.3f), offsetInfo: new Vector2(8f, 5f), offsetCharWielded: new Vector2(2.5f, -0.5f), offsetInfoWielded: new Vector2(2.5f, 1f));
    }

    public static HashSet<IntVector> GetArenaAimingCells(int radius, Thing thingWieldingThis)
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

    public static bool IsPotentialArenaAimingCell(IntVector gridPos, int radius, Thing thingWieldingThis)
    {
        // todo: rething this
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

    public static HashSet<IntVector> GetInventoryAimingCells(Thing thingWieldingThis)
    {
        HashSet<IntVector> aimingCells = new HashSet<IntVector>();

        var player = thingWieldingThis as RoguemojiPlayer;
        if(player == null)
            return aimingCells;

        var gridManager = player.InventoryGridManager;

        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                var gridPos = new IntVector(x, y);
                if (gridManager.GetThingsAtClient(gridPos).Count() == 0)
                    continue;

                aimingCells.Add(gridPos);
            }
        }

        return aimingCells;
    }

    public static bool IsPotentialInventoryAimingCell(IntVector gridPos, Thing thingWieldingThis)
    {
        var player = thingWieldingThis as RoguemojiPlayer;
        if (player == null)
            return false;

        return player.InventoryGridManager.GetThingsAt(gridPos).Count() > 0;
    }
}
