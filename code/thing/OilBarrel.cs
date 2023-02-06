﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class OilBarrel : Thing
{
	public OilBarrel()
	{
		DisplayIcon = "️🛢";
        DisplayName = "Oil Barrel";
        Description = "An open barrel of flammable oil";
        Tooltip = "A barrel of oil";
        IconDepth = (int)IconDepthLevel.Solid;
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 20f;
        Flammability = 30;
    }
}
