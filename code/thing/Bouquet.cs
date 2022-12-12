using Sandbox;
using System;

namespace Roguemoji;
public partial class Bouquet : Thing
{
	public Bouquet()
	{
		DisplayIcon = "💐";
        DisplayName = "Bouquet";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A bouquet of flowers.";
		Flags = ThingFlags.Selectable;
    }
}
