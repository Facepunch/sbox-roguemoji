using Sandbox;
using System;

namespace Roguemoji;
public partial class OilBarrel : Thing
{
	public OilBarrel()
	{
		DisplayIcon = "️🛢";
        DisplayName = "Oil Barrel";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Tooltip = "A flammable barrel of oil.";
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 20f;
    }
}
