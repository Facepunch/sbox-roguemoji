using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum ScrollType { Blink, Teleport, Fear, Telekinesis }
public partial class Scroll : Thing
{
    [Net] public ScrollType ScrollType { get; protected set; }

    public static string UnidentifiedDisplayName => "Scroll of ???";
    public static string UnidentifiedDescription => "An unknown scroll";
    public static string UnidentifiedTooltip => "A scroll of ???";

    public Scroll()
    {
        DisplayIcon = "📜";
        IconDepth = 0;
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        if (user is RoguemojiPlayer player)
            player.IdentifyScroll(this);
    }

    public override void Use(Thing user, Direction direction)
    {
        base.Use(user, direction);

        if (user is RoguemojiPlayer player)
            player.IdentifyScroll(this);
    }

    public override void Use(Thing user, IntVector targetGridPos)
    {
        base.Use(user, targetGridPos);

        if (user is RoguemojiPlayer player)
            player.IdentifyScroll(this);
    }
}
