using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum ScrollType { Blink, Teleport, Fear, Telekinesis }
public partial class Scroll : Thing
{
    [Net] public ScrollType ScrollType { get; protected set; }

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

    public void SetTattoo(string icon)
    {
        SetTattoo(icon, scale: 0.5f, offset: new Vector2(1f, 0f), offsetWielded: new Vector2(0f, 2.3f), offsetInfo: new Vector2(8f, 5f), offsetCharWielded: new Vector2(0.5f, -2.5f), offsetInfoWielded: new Vector2(3f, 2f));
    }
}
