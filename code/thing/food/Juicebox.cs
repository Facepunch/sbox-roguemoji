using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class Juicebox : Thing
{
    public override string AbilityName => "Drink";
    public int HealthAmount { get; private set; }
    public int DurabilityAmount { get; private set; }
    public int DurabilityCost { get; private set; }

    public Juicebox()
	{
		DisplayIcon = "🧃";
        DisplayName = "Juicebox";
        Description = "Heals each time you sip";
        Tooltip = "A juicebox";
        IconDepth = 0;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        if (Game.IsServer)
        {   
            HealthAmount = 1;
            DurabilityAmount = 3;
            DurabilityCost = 1;

            InitStat(StatType.Durability, current: DurabilityAmount, max: DurabilityAmount);

            AddTrait(AbilityName, "😋", $"Take a sip to heal {HealthAmount}{GetStatIcon(StatType.Health)}", offset: new Vector2(0f, -1f), tattooIcon: "🥤", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f));
            AddTrait("", GetStatIcon(StatType.Health), $"Sipping heals {HealthAmount}{GetStatIcon(StatType.Health)}", offset: new Vector2(0f, 0f), labelText: $"+{HealthAmount}", labelFontSize: 18, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", GetStatIcon(StatType.Durability), $"Sipping costs {DurabilityCost}{GetStatIcon(StatType.Durability)}", offset: new Vector2(0f, -3f), labelText: $"-{DurabilityCost}", labelFontSize: 18, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        AdjustStat(StatType.Durability, -DurabilityCost);

        int amountRecovered = Math.Min(HealthAmount, user.GetStatMax(StatType.Health) - user.GetStatClamped(StatType.Health));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Health), user.GridPos, 1.33f, user.CurrentLevelId, new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), new Vector2(Game.Random.Float(-10f, 10f), Game.Random.Float(0f, -10f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, parent: user);
        user.AdjustStat(StatType.Health, HealthAmount);

        base.Use(user);

        if (GetStatClamped(StatType.Durability) == 0)
            Destroy();
    }
}
