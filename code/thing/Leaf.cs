using Sandbox;
using System;

namespace Interfacer;
public partial class Leaf : Thing
{
	public Leaf()
	{
		DisplayIcon = "🍂";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A couple leaves.";
		Flags = ThingFlags.Selectable;
    }
}
