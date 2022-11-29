using Sandbox;
using System;

namespace Interfacer;
public partial class OilBarrel : Thing
{
	public OilBarrel()
	{
		DisplayIcon = "️🛢";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Tooltip = "A flammable barrel of oil.";
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 20f;
    }
}
