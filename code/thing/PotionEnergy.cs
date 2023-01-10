using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionEnergy : Thing
{
    public override string ChatDisplayIcons => $"🧴{GetStatIcon(StatType.Energy)}";
    public override string AbilityName => "Chug Drink";
    public int EnergyAmount { get; private set; }

    public PotionEnergy()
	{
		DisplayIcon = "🧴";
        DisplayName = "Energy Drink";
        Description = "Recover some energy";
        Tooltip = "An energy drink";
        IconDepth = 0;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        SetTattoo(GetStatIcon(StatType.Energy), scale: 0.5f, offset: new Vector2(-0.525f, 4f), offsetWielded: new Vector2(-3f, 2f), offsetInfo: new Vector2(-8.75f, 11.5f), offsetCharWielded: new Vector2(-0.9f, 5.5f), offsetInfoWielded: new Vector2(-6.8f, 6f));

        if (Game.IsServer)
        {
            EnergyAmount = 10;
            AddTrait(AbilityName, "😋", $"Consume drink to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧴", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f));
            AddTrait("", GetStatIcon(StatType.Energy), $"Drinking recovers {EnergyAmount}{GetStatIcon(StatType.Energy)}", offset: new Vector2(0f, -3f), labelText: $"+{EnergyAmount}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (!user.HasStat(StatType.Energy))
            return false;

        return true;
    }

    public override void Use(Thing user)
    {
        int amountRecovered = Math.Min(EnergyAmount, user.GetStatMax(StatType.Energy) - user.GetStatClamped(StatType.Energy));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Energy), user.GridPos, 1.2f, user.CurrentLevelId, new Vector2(0f, 1f), new Vector2(0f, -6f), $"+{amountRecovered}", requireSight: true, EasingType.SineOut, 0.25f, parent: user);
        user.AdjustStat(StatType.Energy, EnergyAmount);

        Destroy();

        base.Use(user);
    }
}
