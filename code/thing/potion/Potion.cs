using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum PotionType { Health, Mana, Energy }
public partial class Potion : Thing
{
    [Net] public PotionType PotionType { get; protected set; }

    public Potion()
    {
        DisplayIcon = "🧉";
        IconDepth = 0;
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        if (user is RoguemojiPlayer player)
            player.IdentifyPotion(this);
    }
}
