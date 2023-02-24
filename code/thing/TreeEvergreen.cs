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
        Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CantBePushed;
        PathfindMovementCost = 99f;
        HealthAmount = 400;
        Flammability = 17;

        if (Game.IsServer)
        {
            InitStat(StatType.SightBlockAmount, 14);
            InitStat(StatType.Health, HealthAmount, min: 0, max: HealthAmount);
        }
    }
    public override void GetSound(SoundActionType actionType, SurfaceType surfaceType, out string sfxName, out int loudness)
    {
        switch (actionType)
        {
            case SoundActionType.GetHit:
                sfxName = "evergreen_tree_hit";
                loudness = 3;
                return;
        }

        base.GetSound(actionType, surfaceType, out sfxName, out loudness);
    }
}
