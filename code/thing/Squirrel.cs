using Sandbox;
using System;

namespace Roguemoji;
public partial class Squirrel : Thing
{
    public Targeting Targeting { get; private set; }
    public Pathfinding Pathfinding { get; private set; }
    public Acting Acting { get; private set; }

    public Squirrel()
	{
		DisplayIcon = "🐿";
        DisplayName = "Squirrel";
        Description = "A bushy-tailed rodent.";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Tooltip = "A squirrel.";
		Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanUseThings;
        PathfindMovementCost = 5f;
        SightBlockAmount = 8;

        InitStat(StatType.Health, 3, 0, 3);
        InitStat(StatType.Attack, 1);
        InitStat(StatType.Speed, 2);
        InitStat(StatType.Sight, 7);
        InitStat(StatType.Hearing, 3);
        //InitStat(StatType.Smell, 3);
    }

    public override void Spawn()
    {
        base.Spawn();

        Targeting = AddThingComponent<Targeting>();
        Pathfinding = AddThingComponent<Pathfinding>();
        Acting = AddThingComponent<Acting>();
        Acting.ActionDelay = 2f;
        Acting.TimeSinceAction = Game.Random.Float(0f, 2f);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (Targeting == null)
            return;

        if (Targeting.Target == null)
        {
            Targeting.Target = RoguemojiGame.Instance.GetClosestPlayer(GridPos);
        }
        else
        {
            if (Acting.IsActionReady)
            {
                var path = Pathfinding.GetPathTo(GridPos, Targeting.Target.GridPos);
                if (path != null && path.Count > 0 && !path[0].Equals(GridPos))
                {
                    var dir = GridManager.GetDirectionForIntVector(path[0] - GridPos);
                    TryMove(dir);
                }

                Acting.PerformedAction();
            }
        }
    }

    public override void TakeDamage(Thing source)
    {
        base.TakeDamage(source);
    }

    public override void Destroy()
    {
        ContainingGridManager.SpawnThing<Bone>(GridPos);

        base.Destroy();
    }
}
