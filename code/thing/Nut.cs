using Sandbox;
using System;

namespace Roguemoji;
public partial class Nut : Thing
{
	public Nut()
	{
		DisplayIcon = "🌰";
        DisplayName = "Nut";
        Description = "Some sort of nut.";
        Tooltip = "A nut.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable;
    }
}
