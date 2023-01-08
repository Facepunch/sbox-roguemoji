using Sandbox;
using System;

namespace Roguemoji;
public partial class SafetyVest : Thing
{
    public int StealthAmount { get; private set; }

    // todo: resist fire

    public SafetyVest()
	{
		DisplayIcon = "🦺";
        DisplayName = "Safety Vest";
        Description = "Highly visible";
        Tooltip = "A safety vest";
        IconDepth = 0;
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;

        if (Game.IsServer)
        {
            StealthAmount = -2;

            InitStat(StatType.Stealth, StealthAmount, min: -999, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        thing.AdjustStat(StatType.Stealth, StealthAmount);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        thing.AdjustStat(StatType.Stealth, -StealthAmount);
    }
}
