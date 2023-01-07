using Sandbox;
using System;

namespace Roguemoji;
public partial class Golem : Thing
{
    public CompTargeting Targeting { get; private set; }
    public CompActing Acting { get; private set; }

    public Golem()
	{
		DisplayIcon = "🗿";
        DisplayName = "Golem";
        Description = "Living rock that moves with mindless intent";
        Tooltip = "A golem";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanUseThings;
        PathfindMovementCost = 15f;
        SightBlockAmount = 12;

        InitStat(StatType.Speed, 2);
        InitStat(StatType.Sight, 7);
        InitStat(StatType.Hearing, 3);
        //InitStat(StatType.Smell, 3);
        FinishInitStats();
    }

    public override void Spawn()
    {
        base.Spawn();

        Targeting = AddComponent<CompTargeting>();
        Acting = AddComponent<CompActing>();
        Acting.ActionDelay = 1.5f;
        Acting.TimeElapsed = Game.Random.Float(0f, 1.5f);
    }
}
