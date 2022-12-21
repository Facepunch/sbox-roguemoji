using Sandbox;
using System;

namespace Roguemoji;
public partial class Coat : Thing
{
	public Coat()
	{
		DisplayIcon = "🧥";
        DisplayName = "Coat";
        Description = "Thick and fashionable.";
        Tooltip = "A thick coat.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;
    }
}
