using Sandbox;
using System;

namespace Roguemoji;
public partial class Hole : Thing
{
	public Hole()
	{
        DisplayIcon = "️🕳";
        DisplayName = "Hole";
        Description = "A deep hole leading to another area";
        Tooltip = "A hole";
        IconDepth = 1;
        ShouldLogBehaviour = true;
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 15f;

        if (Game.IsClient)
        {
            CharSkip = 1;
        }
    }
}
