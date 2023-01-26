using Sandbox;
using System;

namespace Roguemoji;
public partial class Bone : Thing
{
	public Bone()
	{
		DisplayIcon = "🦴";
		DisplayName = "Bone";
        IconDepth = (int)IconDepthLevel.Normal;
		Tooltip = "A bone";
		Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 2);
        }
    }
}
