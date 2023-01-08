using Sandbox;
using System;

namespace Roguemoji;
public partial class TreeEvergreen : Thing
{
	public TreeEvergreen()
	{
		DisplayIcon = "🌲";
        DisplayName = "Evergreen Tree";
        Description = "A tall tree";
        Tooltip = "An evergreen tree";
        IconDepth = 1;
		Flags = ThingFlags.Solid | ThingFlags.Selectable;
		PathfindMovementCost = 999f;
		SightBlockAmount = 14;
    }
}
