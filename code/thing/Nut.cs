using Sandbox;
using System;

namespace Roguemoji;
public partial class Nut : Thing
{
	public Nut()
	{
		DisplayIcon = "🌰";
        DisplayName = "Nut";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A nut.";
		Flags = ThingFlags.Selectable;
    }
}
