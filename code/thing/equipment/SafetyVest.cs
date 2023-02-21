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
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Equipment;
        Flammability = 13;

        if (Game.IsServer)
        {
            StealthAmount = -2;

            InitStat(StatType.Stealth, StealthAmount, min: -999, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        base.OnEquippedTo(thing);

        thing.AdjustStat(StatType.Stealth, StealthAmount);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        base.OnUnequippedFrom(thing);

        thing.AdjustStat(StatType.Stealth, -StealthAmount);
    }
}
