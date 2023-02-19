using Sandbox;
using System;

namespace Roguemoji;
public partial class Bone : Thing
{
    // todo: add durability

	public Bone()
	{
		DisplayIcon = "🦴";
		DisplayName = "Bone";
        IconDepth = (int)IconDepthLevel.Normal;
		Tooltip = "A bone";
		Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;
        Flammability = 8;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 2);
        }
    }
}
