using Sandbox;
using System;

namespace Roguemoji;
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
