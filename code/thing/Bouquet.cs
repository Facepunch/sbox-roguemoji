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
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;
        Flammability = 18;
    }
}
