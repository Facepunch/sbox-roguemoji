using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionEnergy : Potion
{
    public override string ChatDisplayIcons => $"🧉{GetStatIcon(StatType.Energy)}";
    public override string AbilityName => "Quaff Potion";
    public int EnergyAmount { get; private set; }

    public PotionEnergy()
    {
        PotionType = PotionType.Energy;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = "Energy Potion";
        Description = "Recover some energy";
        Tooltip = "An energy potion";

        SetTattoo(GetStatIcon(StatType.Energy), scale: 0.475f, offset: new Vector2(-0.8585f, 4f), offsetWielded: new Vector2(-1.5f, 4f), offsetInfo: new Vector2(-4f, 16f), offsetCharWielded: new Vector2(-2f, 6f), offsetInfoWielded: new Vector2(-4f, 7f));

        if (Game.IsServer)
        {
            EnergyAmount = 10;
            AddTrait(AbilityName, "😋", $"Consume potion to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f));
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
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Energy), GridPos, 1.33f, CurrentLevelId, new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), new Vector2(Game.Random.Float(-10f, 10f), Game.Random.Float(0f, -10f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, parent: user);
        user.AdjustStat(StatType.Energy, EnergyAmount);

        Destroy();

        base.Use(user);
    }
}
