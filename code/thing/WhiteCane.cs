using Sandbox;
using System;

namespace Roguemoji;
public partial class WhiteCane : Thing
{
	public WhiteCane()
	{
		DisplayIcon = "🦯";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A white cane for the vision impaired.";
		Flags = ThingFlags.Selectable;
    }

    public override void OnWieldedBy(Thing thing)
    {
        thing.AdjustStatMin(StatType.Sight, 3);
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        thing.AdjustStatMin(StatType.Sight, -3);
    }
}
