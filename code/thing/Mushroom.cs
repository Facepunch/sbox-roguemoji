using Sandbox;
using System;

namespace Roguemoji;
public partial class Mushroom : Thing
{
	public Mushroom()
	{
		DisplayIcon = "🍄";
        DisplayName = "Mushroom";
        Description = "There's a good chance it's poisonous.";
        Tooltip = "A mushroom.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable;

        if (Game.IsServer)
        {
            AddTrait("", "🍽️", "When eaten, equal chance to poison or temporarily increase sight.");
        }
    }
}
