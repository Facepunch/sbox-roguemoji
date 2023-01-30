using Sandbox;
using System;

namespace Roguemoji;
public partial class Squirrel : Thing
{
    public CTargeting Targeting { get; private set; }
    public CActing Acting { get; private set; }

    public float CantSeeTargetElapsedTime { get; private set; }
    public float CantSeeTargetLoseDelay { get; private set; }
    public IntVector TargetLastSeenPos { get; set; }
    public IntVector WanderGridPos { get; set; }

    public Squirrel()
	{
		DisplayIcon = "🐿";
        DisplayName = "Squirrel";
        Description = "A bushy-tailed rodent";
        IconDepth = (int)IconDepthLevel.Solid;
        ShouldUpdate = true;
		Tooltip = "A squirrel";
		Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanUseThings;
        PathfindMovementCost = 5f;
        Faction = FactionType.Enemy;
        CantSeeTargetLoseDelay = 5f;

        if (Game.IsServer)
        {
            InitStat(StatType.Health, 5, 0, 5);
            InitStat(StatType.Attack, 1);
            InitStat(StatType.Speed, 2);
            InitStat(StatType.Sight, 7);
            InitStat(StatType.Hearing, 3);
            InitStat(StatType.SightBlockAmount, 5);
            //InitStat(StatType.Smell, 3);
        }
    }

    public override void Spawn()
    {
        base.Spawn();

        Targeting = AddComponent<CTargeting>();
        Acting = AddComponent<CActing>();
        Acting.ActionDelay = CActing.CalculateActionDelay(GetStatClamped(StatType.Speed));
        Acting.ActionTimer = Game.Random.Float(0f, 1f);
    }

    //public override void ClientTick()
    //{
    //    base.ClientTick();

    //    DrawDebugText($"{TimeSinceLocalPlayerSaw}");
    //}

    public override void Update(float dt)
    {
        base.Update(dt);

        if (Targeting == null || IsInTransit || IsRemoved)
            return;

        if (!Targeting.HasTarget)
        {
            //RoguemojiGame.Instance.DebugGridLine(GridPos, WanderGridPos, new Color(0.6f, 0f, 1f, 0.8f), 0.025f);

            if (Acting.IsActionReady && !GridPos.Equals(WanderGridPos))
            {
                TryToMove(WanderGridPos);
            }

            //Targeting.Target = RoguemojiGame.Instance.GetClosestPlayer(GridPos);
        }
        else
        {
            var target = Targeting.Target;

            if(target == null || !target.IsValid || (target.HasStat(StatType.Health) && target.GetStatClamped(StatType.Health) <= 0))
            {
                Targeting.LoseTarget();
                WanderGridPos = GridPos;
                return;
            }

            if (target.CurrentLevelId != CurrentLevelId)
            {
                Targeting.Target = null;
            }
            else
            {
                int adjustedSight = Math.Max(GetStatClamped(StatType.Sight) - target.GetStatClamped(StatType.Stealth), 1);
                bool canSeeTarget = CanSeeGridPos(target.GridPos, adjustedSight) && CanSeeThing(target);
                bool isFearful = GetComponent<CFearful>(out var fearful);

                //RoguemojiGame.Instance.DebugGridLine(GridPos, Targeting.Target.GridPos, canSeeTarget ? new Color(0f, 0f, 1f, 0.8f) : new Color(1f, 0f, 0f, 0.8f), 0.025f);

                if (Acting.IsActionReady)
                {
                    if (canSeeTarget)
                        TargetLastSeenPos = target.GridPos;

                    var targetPos = isFearful
                        ? CFearful.GetTargetRetreatPoint(GridPos, ((CFearful)fearful).FearedThing.GridPos, ContainingGridManager)
                        : TargetLastSeenPos;

                    //RoguemojiGame.Instance.DebugGridLine(GridPos, targetPos, canSeeTarget ? new Color(0f, 0f, 1f, 0.2f) : new Color(1f, 0f, 0f, 0.2f), 0.5f, CurrentLevelId);

                    TryToMove(targetPos);
                }

                if(!isFearful)
                {
                    if (canSeeTarget)
                    {
                        CantSeeTargetElapsedTime = 0f;
                    }
                    else
                    {
                        CantSeeTargetElapsedTime += dt;
                        if (CantSeeTargetElapsedTime > CantSeeTargetLoseDelay)
                            Targeting.LoseTarget();
                    }
                }
            }
        }
    }

    void TryToMove(IntVector gridPos)
    {
        var path = GetPathTo(GridPos, gridPos);
        if (path != null && path.Count > 0 && !path[0].Equals(GridPos))
        {
            var dir = GridManager.GetDirectionForIntVector(path[0] - GridPos);

            if (HasComponent<CConfused>() && Game.Random.Int(0, 1) == 0)
                dir = GridManager.GetRandomDirection(cardinalOnly: false);

            TryMove(dir);
        }

        Acting.PerformedAction();
    }

    public override void OnSpawned()
    {
        base.OnSpawned();

        WanderGridPos = GridPos;
    }

    public override void OnFindTarget(Thing target)
    {
        base.OnFindTarget(target);

        RemoveFloater("❔");
        AddFloater("❕", 1.55f, new Vector2(0f, -10f), new Vector2(0f, -35f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.QuadOut, 0.05f);
        Acting.PerformedAction();
        Acting.ActionTimer = Game.Random.Float(0f, 0.1f);
        TargetLastSeenPos = target.GridPos;
    }

    public override void OnLoseTarget()
    {
        base.OnLoseTarget();

        RemoveFloater("❕");
        AddFloater("❔", Game.Random.Float(0.95f, 1.1f), new Vector2(0f, -10f), new Vector2(0f, -30f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.QuadOut, 0.1f);
        Acting.PerformedAction();
        Acting.ActionTimer = Game.Random.Float(0f, 0.1f);

        WanderGridPos = TargetLastSeenPos;
    }

    public override void TakeDamageFrom(Thing thing)
    {
        base.TakeDamageFrom(thing);

        int amount = thing.GetAttackDamage();
        if (amount > 0 && GetStatClamped(StatType.Health) == 1 && !HasComponent<CFearful>())
        {
            var fearful = AddComponent<CFearful>();
            fearful.Lifetime = 5f;

            if (thing.GetComponent<CProjectile>(out var component))
                fearful.FearedThing = ((CProjectile)component).Thrower;
            else
                fearful.FearedThing = thing;
        }
    }

    public override void Destroy()
    {
        // todo: dont spawn blood if burned to death
        if(!ContainingGridManager.DoesGridPosContainThingType<PuddleBlood>(GridPos))
            ContainingGridManager.SpawnThing<PuddleBlood>(GridPos);

        if (Game.Random.Float(0f, 1f) < 0.5f)
            ContainingGridManager.SpawnThing<Bone>(GridPos);

        base.Destroy();
    }
}
