using Sandbox;
using System;

namespace Roguemoji;
public partial class SafetyVest : Thing
{
    public int MaxHealthAmount { get; private set; }
    public int CharismaAmount { get; private set; }

    public SafetyVest()
	{
		DisplayIcon = "🦺";
        DisplayName = "Safety Vest";
        Description = "High visibility, low style.";
        Tooltip = "A safety vest.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;

        if (Game.IsServer)
        {
            MaxHealthAmount = 2;
            CharismaAmount = -1;

            InitStat(StatType.MaxHealth, MaxHealthAmount, isModifier: true);
            InitStat(StatType.Charisma, CharismaAmount, int.MinValue, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        thing.AdjustStatMax(StatType.Health, MaxHealthAmount);
        thing.AdjustStat(StatType.Charisma, CharismaAmount);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        thing.AdjustStatMax(StatType.Health, -MaxHealthAmount);
        thing.AdjustStat(StatType.Charisma, -CharismaAmount);
    }
}
