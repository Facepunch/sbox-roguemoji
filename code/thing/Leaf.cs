using Sandbox;
using System;

namespace Roguemoji;
public partial class Leaf : Thing
{
	public Leaf()
	{
		DisplayIcon = "🍂";
        DisplayName = "Leaves";
        Description = "Small pile of dead leaves.";
        Tooltip = "A pile of leaves.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable;
    }
}
