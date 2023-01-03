using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionMana : Thing
{
    public override string ChatDisplayIcons => "🧴🔮";
    public override string AbilityName => "Drink Potion";
    public int ManaAmount { get; private set; }

    public PotionMana()
	{
		DisplayIcon = "🧴";
        DisplayName = "Mana Potion";
        Description = "Recover some mana.";
        Tooltip = "A mana potion.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
        Flags = ThingFlags.Selectable | ThingFlags.Useable;

        SetTattoo("🔮", scale: 0.43f, offset: new Vector2(-0.6f, 4.5f), offsetWielded: new Vector2(-3f, 2f), offsetInfo: new Vector2(-8.5f, 11.5f), offsetCharWielded: new Vector2(-0.9f, 5.5f), offsetInfoWielded: new Vector2(-6.9f, 6f));

        if (Game.IsServer)
        {
            ManaAmount = 5;
            AddTrait(AbilityName, "😋", "Consume potion to cause an effect.", offset: Vector2.Zero, tattooIcon: "🧴", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f));
            AddTrait("", "🔮", $"Drinking recovers {ManaAmount}🔮", offset: new Vector2(0f, -1f), labelText: $"+{ManaAmount}", labelFontSize: 15, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool TryStartUsing(Thing user)
    {
        if (!user.HasStat(StatType.Mana))
            return false;

        var mana = user.GetStatClamped(StatType.Mana);
        var manaMax = user.GetStatMax(StatType.Mana);
        if (mana == manaMax)
        {
            if (user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You already have max 🔮");

            return false;
        }

        return true;
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        int amountRecovered = Math.Min(ManaAmount, user.GetStatMax(StatType.Mana) - user.GetStatClamped(StatType.Mana));
        RoguemojiGame.Instance.AddFloater("🔮", user.GridPos, 1.2f, user.CurrentLevelId, new Vector2(0f, 1f), new Vector2(0f, -6f), $"+{amountRecovered}", requireSight: true, EasingType.SineOut, 0.25f, parent: user);

        user.AdjustStat(StatType.Mana, ManaAmount);

        Destroy();
    }
}
