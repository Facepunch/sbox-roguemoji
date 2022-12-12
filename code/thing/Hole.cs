using Sandbox;
using System;

namespace Roguemoji;
public partial class Hole : Thing
{
	public Hole()
	{
        DisplayIcon = "️🕳";
        DisplayName = "Hole";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Tooltip = "A hole.";
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 15f;

        if (Game.IsClient)
        {
            CharSkip = 1;
        }
    }
}
