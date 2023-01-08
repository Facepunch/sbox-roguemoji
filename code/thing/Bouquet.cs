using Sandbox;
using System;

namespace Roguemoji;
public partial class Bouquet : Thing
{
	public Bouquet()
	{
		DisplayIcon = "💐";
        DisplayName = "Bouquet";
        Description = "A lovely bunch of flowers";
        Tooltip = "A bouquet";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable;
    }
}
