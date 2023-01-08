using Sandbox;
using System;

namespace Roguemoji;
public partial class Golem : Thing
{
    public CTargeting Targeting { get; private set; }
    public CActing Acting { get; private set; }

    public Golem()
	{
		DisplayIcon = "🗿";
        DisplayName = "Golem";
        Description = "Living rock that moves with mindless intent";
        Tooltip = "A golem";
        IconDepth = 1;
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

        Targeting = AddComponent<CTargeting>();
        Acting = AddComponent<CActing>();
        Acting.ActionDelay = 1.5f;
        Acting.TimeElapsed = Game.Random.Float(0f, 1.5f);
    }
}
