using Sandbox;
using System;

namespace Roguemoji;
public partial class Coat : Thing
{
    public int MaxHealthAmount { get; private set; }

    public Coat()
	{
		DisplayIcon = "🧥";
        DisplayName = "Coat";
        Description = "Thick and warm.";
        Tooltip = "A thick coat.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;

        if (Game.IsServer)
        {
            MaxHealthAmount = 3;

            InitStat(StatType.MaxHealth, MaxHealthAmount, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        thing.AdjustStatMax(StatType.Health, MaxHealthAmount);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        thing.AdjustStatMax(StatType.Health, -MaxHealthAmount);
    }
}
