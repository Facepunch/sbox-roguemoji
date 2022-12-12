using Sandbox;
using System;

namespace Roguemoji;
public partial class Telescope : Thing
{
	public Telescope()
	{
		DisplayIcon = "🔭";
        DisplayName = "Telescope";
        Description = "See farther but move slower.";
        Tooltip = "A telescope.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable;
    }

    public override void OnWieldedBy(Thing thing)
    {
        thing.AdjustStat(StatType.Sight, 6);
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        thing.AdjustStat(StatType.Sight, -6);
    }
}
