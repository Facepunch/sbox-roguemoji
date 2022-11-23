using Sandbox;
using System;

namespace Interfacer;
public partial class Bouquet : Thing
{
	public Bouquet()
	{
		DisplayIcon = "💐";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A bouquet of flowers.";
		Flags = ThingFlags.Selectable;
    }
}
