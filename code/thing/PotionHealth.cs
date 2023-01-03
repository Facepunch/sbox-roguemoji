using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionHealth : Thing
{
    public override string ChatDisplayIcons => "🧉❤️";
    public override string AbilityName => "Drink Potion";
    public int HealthAmount { get; private set; }

    public PotionHealth()
	{
		DisplayIcon = "🧉";
        DisplayName = "Health Potion";
        Description = "Recover some health.";
        Tooltip = "A health potion.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
        Flags = ThingFlags.Selectable | ThingFlags.Useable;

        SetTattoo("❤️", scale: 0.5f, offset: new Vector2(-0.8575f, 6.5f), offsetWielded: new Vector2(-1.5f, 4f), offsetInfo: new Vector2(-4f, 16f), offsetCharWielded: new Vector2(-0.9f, 9.5f), offsetInfoWielded: new Vector2(-4f, 8f));

        if (Game.IsServer)
        {
            HealthAmount = 5;
            AddTrait(AbilityName, "😋", "Consume potion to cause an effect.", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f));
            AddTrait("", "❤️", $"Drinking recovers {HealthAmount}❤️", offset: new Vector2(0f, -1f), labelText: $"+{HealthAmount}", labelFontSize: 15, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool TryStartUsing(Thing user)
    {
        if (!user.HasStat(StatType.Health))
            return false;

        var health = user.GetStatClamped(StatType.Health);
        var healthMax = user.GetStatMax(StatType.Health);
        if (health == healthMax)
        {
            if (user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You already have max ❤️");

            return false;
        }

        return true;
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        int amountRecovered = Math.Min(HealthAmount, user.GetStatMax(StatType.Health) - user.GetStatClamped(StatType.Health));
        RoguemojiGame.Instance.AddFloater("❤️", user.GridPos, 1.2f, user.CurrentLevelId, new Vector2(0f, 1f), new Vector2(0f, -6f), $"+{amountRecovered}", requireSight: true, EasingType.SineOut, 0.25f, parent: user);

        user.AdjustStat(StatType.Health, HealthAmount);

        Destroy();
    }
}
