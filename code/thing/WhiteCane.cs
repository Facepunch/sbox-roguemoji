using Sandbox;
using System;

namespace Roguemoji;
public partial class WhiteCane : Thing
{
	public WhiteCane()
	{
		DisplayIcon = "🦯";
        DisplayName = "White Cane";
        Description = "Prevents your sight from reaching zero.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A white cane for the vision impaired.";
		Flags = ThingFlags.Selectable;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
            //AddTrait("", "👁️", "Prevents your 👁️ from reaching zero.");
        }
    }

    public override void OnWieldedBy(Thing thing)
    {
        thing.AdjustStatMin(StatType.Sight, 3);
        thing.AdjustStat(StatType.Attack, GetStat(StatType.Attack));
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        thing.AdjustStatMin(StatType.Sight, -3);
        thing.AdjustStat(StatType.Attack, -GetStat(StatType.Attack));
    }
}
