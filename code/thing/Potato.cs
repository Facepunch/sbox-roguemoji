using Sandbox;
using System;

namespace Interfacer;
public partial class Potato : Thing
{
	public Potato()
	{
		DisplayIcon = "🥔";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A potato.";
		Flags = ThingFlags.Selectable;
    }
}
