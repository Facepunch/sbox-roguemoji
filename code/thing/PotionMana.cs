using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionMana : Thing
{
    public override string ChatDisplayIcons => $"🧉{GetStatIcon(StatType.Mana)}";
    public override string AbilityName => "Drink Potion";
    public int ManaAmount { get; private set; }

    public PotionMana()
	{
		DisplayIcon = "\U0001f9c9";
        DisplayName = "Mana Potion";
        Description = "Recover some mana";
        Tooltip = "A mana potion";
        IconDepth = 0;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        SetTattoo(GetStatIcon(StatType.Mana), scale: 0.475f, offset: new Vector2(-0.85f, 6.5f), offsetWielded: new Vector2(-1.5f, 4f), offsetInfo: new Vector2(-4f, 16f), offsetCharWielded: new Vector2(-0.9f, 9.5f), offsetInfoWielded: new Vector2(-4f, 7f));

        if (Game.IsServer)
        {
            ManaAmount = 5;
            AddTrait(AbilityName, "😋", $"Consume potion to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f));
            AddTrait("", GetStatIcon(StatType.Mana), $"Drinking recovers {ManaAmount}{GetStatIcon(StatType.Mana)}", offset: new Vector2(0f, -3f), labelText: $"+{ManaAmount}", labelFontSize: 16, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (!user.HasStat(StatType.Mana))
            return false;

        return true;
    }

    public override void Use(Thing user)
    {
        int amountRecovered = Math.Min(ManaAmount, user.GetStatMax(StatType.Mana) - user.GetStatClamped(StatType.Mana));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Mana), user.GridPos, 1.2f, user.CurrentLevelId, new Vector2(0f, 1f), new Vector2(0f, -6f), $"+{amountRecovered}", requireSight: true, EasingType.SineOut, 0.25f, parent: user);

        user.AdjustStat(StatType.Mana, ManaAmount);

        Destroy();

        base.Use(user);
    }
}
