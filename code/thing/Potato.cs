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

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
        }
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        user.AdjustStat(StatType.Health, 2);
        Destroy();
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
