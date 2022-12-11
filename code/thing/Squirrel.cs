using Sandbox;
using System;

namespace Roguemoji;
public partial class Squirrel : Thing
{
    public Targeting Targeting { get; private set; }
    public Pathfinding Pathfinding { get; private set; }

    public TimeSince TimeSinceAction { get; private set; }
    public float ActionDelay { get; private set; }

    public Squirrel()
	{
		DisplayIcon = "🐿";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Tooltip = "A squirrel.";
		Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 5f;
        TimeSinceAction = 0f;
        ActionDelay = Game.Random.Float(1f, 3f);
        SightBlockAmount = 8;

        InitStat(StatType.Health, 3, 0, 3);
        InitStat(StatType.Attack, 1, 0, 10);
        InitStat(StatType.Sight, 7, 0, 10);
        InitStat(StatType.Hearing, 7, 0, 10);
        InitStat(StatType.Smell, 7, 0, 10);
    }

    public override void Spawn()
    {
        base.Spawn();

        Targeting = AddThingComponent<Targeting>();
        Pathfinding = AddThingComponent<Pathfinding>();
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
            if (TimeSinceAction > ActionDelay)
            {
                var path = Pathfinding.GetPathTo(GridPos, Targeting.Target.GridPos);
                if (path != null && path.Count > 0 && !path[0].Equals(GridPos))
                {
                    var dir = GridManager.GetDirectionForIntVector(path[0] - GridPos);
                    TryMove(dir);
                }

                TimeSinceAction = 0f;
            }
        }
    }

    public override void Damage(int amount, Thing source)
    {
        base.Damage(amount, source);
    }

    public override void Destroy()
    {
        ContainingGridManager.SpawnThing<Bone>(GridPos);

        base.Destroy();
    }
}
