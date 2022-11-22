using Sandbox;
using System;

namespace Interfacer;
public partial class Mushroom : Thing
{
	public Mushroom()
	{
		DisplayIcon = "🍄";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A mushroom.";
		Flags = ThingFlags.Selectable;
    }
}
