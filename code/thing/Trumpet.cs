using Sandbox;
using System;

namespace Roguemoji;
public partial class Trumpet : Thing
{
	public Trumpet()
	{
		DisplayIcon = "🎺";
        DisplayName = "Trumpet";
        Description = "Loud and annoying";
        Tooltip = "A trumpet";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable;
    }
}
