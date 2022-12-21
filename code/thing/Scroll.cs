using Sandbox;
using System;

namespace Roguemoji;
public partial class Scroll : Thing
{
	public Scroll()
	{
		DisplayIcon = "📜";
        DisplayName = "Scroll";
        Description = "Blink to a target location nearby.";
        Tooltip = "A magical scroll.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable | ThingFlags.UseRequiresAiming;
    }

    public override void Use(Thing target)
    {
        base.Use(target);

        Destroy();
    }
}
