using Sandbox;
using System;

namespace Roguemoji;
public partial class Cheese : Thing
{
	public Cheese()
	{
		DisplayIcon = "🧀";
        DisplayName = "Cheese";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A wedge of cheese.";
		Flags = ThingFlags.Selectable;
    }
}
