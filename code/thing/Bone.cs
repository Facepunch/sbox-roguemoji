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
            InitStat(StatType.Attack, 1);
        }
    }

    public override void OnWieldedBy(Thing thing)
    {
        base.OnWieldedBy(thing);
        thing.AdjustStat(StatType.Attack, GetStatClamped(StatType.Attack));
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        base.OnNoLongerWieldingThing(thing);
        thing.AdjustStat(StatType.Attack, -GetStatClamped(StatType.Attack));
    }
}
