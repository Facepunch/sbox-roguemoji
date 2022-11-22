using Sandbox;
using System;

namespace Interfacer;
public partial class Nut : Thing
{
	public Nut()
	{
		DisplayIcon = "🌰";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A nut.";
		Flags = ThingFlags.Selectable;
    }
}
