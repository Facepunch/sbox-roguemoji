﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class Rock : Thing
{
	public Rock()
	{
		DisplayIcon = "🗿";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Tooltip = "A rock.";
		Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 15f;
        SightBlockAmount = 15;
    }
}
