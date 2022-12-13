using Sandbox;
using System;

namespace Roguemoji;
public partial class Cheese : Thing
{
	public Cheese()
	{
		DisplayIcon = "🧀";
        DisplayName = "Cheese";
        Tooltip = "A wedge of cheese.";
        Description = "Brightly colored and highly noticeable.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable;
    }
}
