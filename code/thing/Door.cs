﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class Door : Thing
{
	public Door()
	{
		DisplayIcon = "️🚪";
        DisplayName = "Door";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Tooltip = "A door.";
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 15f;
        SightBlockAmount = 20;
    }
}
