using Sandbox;
using System;

namespace Roguemoji;
public partial class Coat : Thing
{
	public Coat()
	{
		DisplayIcon = "🧥";
        DisplayName = "Coat";
        Description = "Brightly colored and highly noticeable.";
        Tooltip = "A thick coat.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;
    }
}
