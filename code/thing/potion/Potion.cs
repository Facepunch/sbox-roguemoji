using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum PotionType { Health, Mana, Energy, Poison }
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

    public void SetTattoo(string icon)
    {
        SetTattoo(icon, scale: 0.475f, offset: new Vector2(-0.8585f, 6f), offsetWielded: new Vector2(-1.5f, 6f), offsetInfo: new Vector2(-3f, 16f), offsetCharWielded: new Vector2(-2f, 6f), offsetInfoWielded: new Vector2(-4f, 7f));
    }
}
