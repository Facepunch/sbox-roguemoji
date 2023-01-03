using Sandbox;
using System;

namespace Roguemoji;
public partial class TreeDeciduous : Thing
{
	public TreeDeciduous()
	{
		DisplayIcon = "🌳";
        DisplayName = "Deciduous Tree";
        Description = "A tall tree.";
        Tooltip = "A deciduous tree.";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Solid | ThingFlags.Selectable;
		PathfindMovementCost = 999f;
		SightBlockAmount = 13;
    }
}
