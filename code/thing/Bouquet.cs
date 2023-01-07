using Sandbox;
using System;

namespace Roguemoji;
public partial class Bouquet : Thing
{
	public Bouquet()
	{
		DisplayIcon = "💐";
        DisplayName = "Bouquet";
        Description = "Lovely bunch of flowers";
        Tooltip = "A bouquet of flowers";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable;
    }
}
