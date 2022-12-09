using Sandbox;
using System;

namespace Roguemoji;
public partial class Coat : Thing
{
	public Coat()
	{
		DisplayIcon = "🧥";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A thick coat.";
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;
    }
}
