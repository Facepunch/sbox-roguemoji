using Sandbox;
using System;

namespace Roguemoji;
public partial class Door : Thing
{
	public Door()
	{
		DisplayIcon = "️🚪";
        DisplayName = "Door";
        IconDepth = (int)IconDepthLevel.Solid;
        Tooltip = "A door";
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 15f;
        Flammability = 0;

        if (Game.IsServer)
        {
            InitStat(StatType.SightBlockAmount, 20);
        }
    }
}
