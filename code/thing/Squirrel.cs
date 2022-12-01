using Sandbox;
using System;

namespace Interfacer;
public partial class Squirrel : Thing
{
    public TargetingStatus Targeting { get; private set; }
    public PathfindingStatus Pathfinding { get; private set; }

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
        ActionDelay = Rand.Float(1f, 3f);
        Hp = MaxHp = 3;
    }

    public override void Spawn()
    {
        base.Spawn();

        Targeting = AddStatus<TargetingStatus>();
        Pathfinding = AddStatus<PathfindingStatus>();
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (Targeting == null)
            return;

        if (Targeting.Target == null)
        {
            Targeting.Target = InterfacerGame.Instance.GetClosestPlayer(GridPos);
        }
        else
        {
            if (TimeSinceAction > ActionDelay)
            {
                var path = Pathfinding.GetPathTo(GridPos, Targeting.Target.GridPos);
                if (path != null && path.Count > 0)
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
        //Tooltip = $"A squirrel.\n{Hp}/{MaxHp}❤️";
    }

    public override void Destroy()
    {
        ContainingGridManager.SpawnThing<Bone>(GridPos);

        base.Destroy();
    }
}
