using Sandbox;
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
        IconDepth = 1;
        ShouldLogBehaviour = true;
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 20f;
    }
}
