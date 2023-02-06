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
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;
        Flammability = 16;

        if (Game.IsServer)
        {   
            HealthAmount = 1;
            DurabilityAmount = 3;
            DurabilityCost = 1;

            InitStat(StatType.Durability, current: DurabilityAmount, max: DurabilityAmount);

            AddTrait(AbilityName, "😋", $"Take a sip to heal {HealthAmount}{GetStatIcon(StatType.Health)}", offset: new Vector2(0f, -1f), tattooIcon: "🥤", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f), isAbility: true);
            AddTrait("", GetStatIcon(StatType.Health), $"Sipping heals {HealthAmount}{GetStatIcon(StatType.Health)}", offset: new Vector2(0f, 0f), labelText: $"+{HealthAmount}", labelFontSize: 18, labelOffset: new Vector2(0f, -1f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", GetStatIcon(StatType.Durability), $"Ability costs {DurabilityCost}{GetStatIcon(StatType.Durability)}", offset: new Vector2(0f, -3f), labelText: $"-{DurabilityCost}", labelFontSize: 18, labelOffset: new Vector2(0f, -1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        AdjustStat(StatType.Durability, -DurabilityCost);

        int amountRecovered = Math.Min(HealthAmount, user.GetStatMax(StatType.Health) - user.GetStatClamped(StatType.Health));
        user.AddFloater(GetStatIcon(StatType.Health), 1.33f, new Vector2(Game.Random.Float(8f, 12f) * (user.FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (user.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f);
        user.AdjustStat(StatType.Health, HealthAmount);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😋", (int)PlayerIconPriority.EatReaction, 0.5f);

        base.Use(user);

        if (GetStatClamped(StatType.Durability) == 0)
            Destroy();
    }
}
