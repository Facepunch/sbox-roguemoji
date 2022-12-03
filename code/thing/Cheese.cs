using Sandbox;
using System;

namespace Roguemoji;
public partial class Cheese : Thing
{
	public Cheese()
	{
		DisplayIcon = "🧀";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A wedge of cheese.";
		Flags = ThingFlags.Selectable;
    }
}
