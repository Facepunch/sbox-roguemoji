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
        HealthAmount = 400;
        Flammability = 17;

        if (Game.IsServer)
        {
            InitStat(StatType.SightBlockAmount, 14);
            InitStat(StatType.Health, HealthAmount, min: 0, max: HealthAmount);
        }
    }
}
