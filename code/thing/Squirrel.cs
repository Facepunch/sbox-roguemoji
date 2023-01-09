using Sandbox;
using System;

namespace Roguemoji;
public partial class Squirrel : Thing
{
    public CTargeting Targeting { get; private set; }
    public CActing Acting { get; private set; }

    public float CantSeeTargetElapsedTime { get; private set; }
    public float CantSeeTargetLoseDelay { get; private set; }

    public Squirrel()
	{
		DisplayIcon = "🐿";
        DisplayName = "Squirrel";
        Description = "A bushy-tailed rodent";
        IconDepth = 1;
        ShouldUpdate = true;
		Tooltip = "A squirrel";
		Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanUseThings;
        PathfindMovementCost = 5f;
        SightBlockAmount = 5;
        Faction = FactionType.Enemy;
        CantSeeTargetLoseDelay = 5f;

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

        Targeting = AddComponent<CTargeting>();
        Acting = AddComponent<CActing>();
        Acting.ActionDelay = 1f;
        Acting.TimeElapsed = Game.Random.Float(0f, 1f);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (Targeting == null || IsInTransit)
            return;

        if (!Targeting.HasTarget)
        {
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
                int adjustedSight = Math.Max(GetStatClamped(StatType.Sight) - Targeting.Target.GetStatClamped(StatType.Stealth), 1);
                bool cantSeeTarget = Utils.GetDistance(GridPos, Targeting.Target.GridPos) > adjustedSight || !ContainingGridManager.HasLineOfSight(GridPos, Targeting.Target.GridPos, adjustedSight, out IntVector collisionCell);
                bool isFearful = GetComponent<CFearful>(out var fearful);

                if (Acting.IsActionReady)
                {
                    var targetPos = isFearful
                        ? CFearful.GetTargetRetreatPoint(GridPos, ((CFearful)fearful).FearedThing.GridPos, ContainingGridManager)
                        : Targeting.Target.GridPos;

                    //RoguemojiGame.Instance.DebugGridLine(GridPos, targetPos, cantSeeTarget ? new Color(1f, 0f, 0f, 0.2f) : new Color(0f, 0f, 1f, 0.2f), 0.5f, CurrentLevelId);

                    var path = GetPathTo(GridPos, targetPos);
                    if (path != null && path.Count > 0 && !path[0].Equals(GridPos))
                    {
                        var dir = GridManager.GetDirectionForIntVector(path[0] - GridPos);
                        TryMove(dir);
                    }

                    Acting.PerformedAction();
                }

                if(!isFearful)
                {
                    if (cantSeeTarget)
                    {
                        CantSeeTargetElapsedTime += dt;
                        if (CantSeeTargetElapsedTime > CantSeeTargetLoseDelay)
                            Targeting.LoseTarget();
                    }
                    else
                    {
                        CantSeeTargetElapsedTime = 0f;
                    }
                }
            }
        }
    }

    public override void OnFindTarget(Thing target)
    {
        base.OnFindTarget(target);

        RoguemojiGame.Instance.RemoveFloater("❔", CurrentLevelId, parent: this);
        RoguemojiGame.Instance.AddFloater("❕", GridPos, 1.55f, CurrentLevelId, new Vector2(0f, -10f), new Vector2(0f, -35f), text: "", requireSight: true, EasingType.QuadOut, 0.05f, parent: this);
        Acting.PerformedAction();
        Acting.TimeElapsed = Game.Random.Float(0f, 0.1f);
    }

    public override void OnLoseTarget()
    {
        base.OnLoseTarget();

        RoguemojiGame.Instance.RemoveFloater("❕", CurrentLevelId, parent: this);
        RoguemojiGame.Instance.AddFloater("❔", GridPos, Game.Random.Float(0.95f, 1.1f), CurrentLevelId, new Vector2(0f, -10f), new Vector2(0f, -30f), text: "", requireSight: false, EasingType.QuadOut, 0.1f, parent: this);
        Acting.PerformedAction();
        Acting.TimeElapsed = Game.Random.Float(0f, 0.1f);
    }

    public override void TakeDamage(Thing source)
    {
        base.TakeDamage(source);

        int amount = source.GetStatClamped(StatType.Attack);

        if (amount > 0 && GetStatClamped(StatType.Health) == 1 && !HasComponent<CFearful>())
        {
            var fearful = AddComponent<CFearful>();
            fearful.Lifetime = 5f;

            if (source.GetComponent<CProjectile>(out var component))
                fearful.FearedThing = ((CProjectile)component).Thrower;
            else
                fearful.FearedThing = source;
        }
    }

    public override void Destroy()
    {
        if(Game.Random.Float(0f, 1f) < 0.5f)
            ContainingGridManager.SpawnThing<Bone>(GridPos);

        base.Destroy();
    }
}
