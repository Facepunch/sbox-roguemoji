using Sandbox;
using System;

namespace Roguemoji;
public partial class TreeEvergreen : Thing
{
	public TreeEvergreen()
	{
		DisplayIcon = "🌲";
        DisplayName = "Tree";
        Description = "A tall evergreen tree";
        Tooltip = "A tree";
        IconDepth = (int)IconDepthLevel.Solid;
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
		PathfindMovementCost = 999f;
		SightBlockAmount = 14;
    }

    public override void TakeDamageFrom(Thing source)
    {
        int amount = source.GetStatClamped(StatType.Attack);

        if (amount <= 0)
            return;

        if (!HasStat(StatType.Health))
            InitStat(StatType.Health, 100, min: 0, max: 100);

        base.TakeDamageFrom(source);
    }
}
