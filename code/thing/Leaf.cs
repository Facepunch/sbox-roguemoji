using Sandbox;
using System;

namespace Roguemoji;
public partial class Leaf : Thing
{
	public Leaf()
	{
		DisplayIcon = "🍂";
        DisplayName = "Leaves";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A couple leaves.";
		Flags = ThingFlags.Selectable;
    }
}
