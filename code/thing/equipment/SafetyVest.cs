using Sandbox;
using System;

namespace Roguemoji;
public partial class SafetyVest : Thing
{
	public SafetyVest()
	{
		DisplayIcon = "🦺";
        DisplayName = "Safety Vest";
        Description = "High visibility, low style.";
        Tooltip = "A safety vest.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;
    }
}
