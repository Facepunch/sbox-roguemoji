using Sandbox;
using System;

namespace Roguemoji;
public partial class Telescope : Thing
{
	public Telescope()
	{
		DisplayIcon = "🔭";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A telescope.";
		Flags = ThingFlags.Selectable;
    }

    public override void OnWieldedBy(Thing thing)
    {
        thing.AdjustStat(ThingStat.Sight, 6);
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        thing.AdjustStat(ThingStat.Sight, -6);
    }
}
