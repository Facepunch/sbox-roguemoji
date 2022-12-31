using Sandbox;
using System;

namespace Roguemoji;
public partial class TreeDeciduous : Thing
{
	public TreeDeciduous()
	{
		DisplayIcon = "🌳";
        DisplayName = "Deciduous Tree";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Tooltip = "A deciduous tree.";
		Flags = ThingFlags.Solid | ThingFlags.Selectable;
		PathfindMovementCost = 999f;
        ShouldUpdate = false;
		SightBlockAmount = 13;
    }
}
