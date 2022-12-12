using Sandbox;
using System;

namespace Roguemoji;
public partial class Mushroom : Thing
{
	public Mushroom()
	{
		DisplayIcon = "🍄";
        DisplayName = "Mushroom";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A mushroom.";
		Flags = ThingFlags.Selectable;
    }
}
