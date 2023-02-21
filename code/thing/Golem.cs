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
        IconDepth = (int)IconDepthLevel.Solid;
        Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanWieldThings;
        PathfindMovementCost = 15f;
        Flammability = 0;

        if (Game.IsServer)
        {
            InitStat(StatType.Speed, 2);
            InitStat(StatType.SightDistance, 7);
            InitStat(StatType.SightPenetration, 7);
            InitStat(StatType.Hearing, 3);
            InitStat(StatType.SightBlockAmount, 20);
        }
    }

    public override void Spawn()
    {
        base.Spawn();

        Targeting = AddComponent<CTargeting>();
        Acting = AddComponent<CActing>();
        Acting.ActionDelay = 1.5f;
        Acting.ActionTimer = Game.Random.Float(0f, 1.5f);
    }
}
