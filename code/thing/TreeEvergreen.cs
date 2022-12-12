using Sandbox;
using System;

namespace Roguemoji;
public partial class TreeEvergreen : Thing
{
	public TreeEvergreen()
	{
		DisplayIcon = "🌲";
        DisplayName = "Evergreen Tree";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Tooltip = "An evergreen tree.";
		Flags = ThingFlags.Solid | ThingFlags.Selectable;
		PathfindMovementCost = 999f;
        ShouldUpdate = false;
		SightBlockAmount = 14;
    }
}
