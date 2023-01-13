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

        SetTattoo(GetStatIcon(StatType.Energy));

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
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Energy), user.GridPos, 1.33f, user.CurrentLevelId, new Vector2(Game.Random.Float(8f, 12f) * (user.FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (user.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, parent: user);
        user.AdjustStat(StatType.Energy, EnergyAmount);

        Destroy();

        base.Use(user);
    }
}
