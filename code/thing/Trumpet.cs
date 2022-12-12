using Sandbox;
using System;

namespace Roguemoji;
public partial class Trumpet : Thing
{
	public Trumpet()
	{
		DisplayIcon = "🎺";
        DisplayName = "Trumpet";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A trumpet.";
		Flags = ThingFlags.Selectable;
    }
}
