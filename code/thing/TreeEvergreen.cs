﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class TreeEvergreen : Thing
{
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
    }
}
