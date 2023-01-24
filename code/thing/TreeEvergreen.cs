using Sandbox;
using System;

namespace Roguemoji;
public partial class TreeEvergreen : Thing
{
    public int HealthAmount { get; set; }

	public TreeEvergreen()
	{
		DisplayIcon = "🌲";
        DisplayName = "Tree";
        Description = "A tall evergreen tree";
        Tooltip = "A tree";
        IconDepth = (int)IconDepthLevel.Solid;
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
		PathfindMovementCost = 999f;
		SightBlockAmount = 14;
        HealthAmount = 400;
    }

    public override void Hurt(int amount, bool showImpactFloater = true)
    {
        if (amount <= 0)
            return;

        if (!HasStat(StatType.Health))
            InitStat(StatType.Health, HealthAmount, min: 0, max: HealthAmount);

        base.Hurt(amount, showImpactFloater);
    }
}
