using Sandbox;
using System;

namespace Roguemoji;
public partial class TreeEvergreen : Thing
{
    public int HealthAmount { get; set; }

    private float _timer;

    public TreeEvergreen()
    {
        DisplayIcon = "🌲";
        DisplayName = "Tree";
        Description = "A tall evergreen tree";
        Tooltip = "A tree";
        IconDepth = (int)IconDepthLevel.Solid;
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 999f;
        HealthAmount = 400;

        if (Game.IsServer)
        {
            InitStat(StatType.SightBlockAmount, 14);
        }
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
