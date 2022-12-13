using Sandbox;
using System;

namespace Roguemoji;
public partial class Potato : Thing
{
	public Potato()
	{
		DisplayIcon = "🥔";
        DisplayName = "Potato";
        Description = "Uncooked and as hard as a rock.";
        Tooltip = "A potato.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable;
    }

    public override void Use(Thing target)
    {
        base.Use(target);

        target.AdjustStat(StatType.Health, 2);
        Destroy();
    }
}
