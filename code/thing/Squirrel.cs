using Sandbox;
using System;

namespace Roguemoji;
public partial class Squirrel : Thing
{
    public CompTargeting Targeting { get; private set; }
    public CompActing Acting { get; private set; }

    public Squirrel()
	{
		DisplayIcon = "🐿";
        DisplayName = "Squirrel";
        Description = "A bushy-tailed rodent.";
        IconDepth = 1;
        ShouldUpdate = true;
        ShouldLogBehaviour = true;
		Tooltip = "A squirrel.";
		Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanUseThings;
        PathfindMovementCost = 5f;
        SightBlockAmount = 8;
        Faction = FactionType.Enemy;

        InitStat(StatType.Health, 5, 0, 5);
        InitStat(StatType.Attack, 1);
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
        Acting.ActionDelay = 1f;
        Acting.TimeElapsed = Game.Random.Float(0f, 1f);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (Targeting == null)
            return;

        if (!Targeting.HasTarget)
        {
            Targeting.CheckForTarget();
            //Targeting.Target = RoguemojiGame.Instance.GetClosestPlayer(GridPos);
        }
        else
        {
            if (Targeting.Target.CurrentLevelId != CurrentLevelId)
            {
                Targeting.Target = null;
            }
            else
            {
                //Color color = ContainingGridManager.HasLineOfSight(GridPos, Targeting.Target.GridPos, GetStatClamped(StatType.Sight), out IntVector collisionCell) ? new Color(0f, 0f, 1f, 0.1f) : new Color(1f, 0f, 0f, 0.1f);
                //RoguemojiGame.Instance.DebugGridLine(GridPos, collisionCell, color, 0.05f, CurrentLevelId);

                if (Acting.IsActionReady)
                {
                    var path = GetPathTo(GridPos, Targeting.Target.GridPos);
                    if (path != null && path.Count > 0 && !path[0].Equals(GridPos))
                    {
                        var dir = GridManager.GetDirectionForIntVector(path[0] - GridPos);
                        TryMove(dir);
                    }

                    Acting.PerformedAction();
                }
            }
        }
    }

    public override void OnFindTarget(Thing target)
    {
        base.OnFindTarget(target);

        RoguemojiGame.Instance.AddFloater("❕", GridPos, 1.55f, CurrentLevelId, new Vector2(0f, -10f), new Vector2(0f, -35f), text: "", requireSight: true, EasingType.QuadOut, 0.1f, parent: this);
        Acting.PerformedAction();
        Acting.TimeElapsed = Game.Random.Float(0f, 0.1f);
    }

    public override void TakeDamage(Thing source)
    {
        base.TakeDamage(source);
    }

    public override void Destroy()
    {
        if(Game.Random.Float(0f, 1f) < 0.5f)
            ContainingGridManager.SpawnThing<Bone>(GridPos);

        base.Destroy();
    }
}
