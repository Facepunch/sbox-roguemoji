using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class SquirrelBrain : ThingBrain
{
    public float CantSeeTargetElapsedTime { get; private set; }
    public float CantSeeTargetLoseDelay { get; private set; }
    public IntVector TargetLastSeenPos { get; set; }
    public IntVector WanderGridPos { get; set; }

    public SquirrelBrain()
    {
        WanderGridPos = new IntVector(1, 1);
    }

    public override void ControlThing(Thing thing)
    {
        base.ControlThing(thing);

        WanderGridPos = ControlledThing.GridPos;
        CantSeeTargetLoseDelay = 5f;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        //RoguemojiGame.Instance.DebugGridLine(ControlledThing.GridPos, TargetLastSeenPos, new Color(0f, 1f, 0f, 0.3f), 0.025f);

        //ControlledThing.DebugText = $"{WanderGridPos}";
        //DebugText = WieldedThing == null ? "null" : WieldedThing.DisplayName;
        CActing acting = null;
        if (ControlledThing.GetComponent<CActing>(out var component))
            acting = (CActing)component;

        CTargeting targeting = null;
        if (ControlledThing.GetComponent<CTargeting>(out var component2))
            targeting = (CTargeting)component2;

        if (acting == null || targeting == null || ControlledThing.IsInTransit || ControlledThing.IsRemoved)
            return;

        if (!targeting.HasTarget)
        {
            //RoguemojiGame.Instance.DebugGridLine(ControlledThing.GridPos, IntVector.Zero, new Color(0.6f, 0f, 1f, 0.8f), 0.025f);

            if (acting.IsActionReady && !ControlledThing.GridPos.Equals(WanderGridPos))
            {
                TryToMoveToPos(WanderGridPos);
            }

            //Targeting.Target = RoguemojiGame.Instance.GetClosestPlayer(GridPos);
        }
        else
        {
            var target = targeting.Target;

            if (target == null || !target.IsValid || (target.HasStat(StatType.Health) && target.GetStatClamped(StatType.Health) <= 0))
            {
                targeting.LoseTarget();
                WanderGridPos = ControlledThing.GridPos;
                return;
            }

            if (target.CurrentLevelId != ControlledThing.CurrentLevelId)
            {
                targeting.Target = null;
            }
            else
            {
                int adjustedSight = Math.Max(ControlledThing.GetStatClamped(StatType.Sight) - target.GetStatClamped(StatType.Stealth), 1);
                bool canSeeTarget = ControlledThing.CanSeeGridPos(target.GridPos, adjustedSight) && ControlledThing.CanPerceiveThing(target);
                bool isFearful = ControlledThing.GetComponent<CFearful>(out var fearful);

                //RoguemojiGame.Instance.DebugGridLine(ControlledThing.GridPos, targeting.Target.GridPos, canSeeTarget ? new Color(0f, 0f, 1f, 0.8f) : new Color(1f, 0f, 0f, 0.8f), 0.025f);

                if (acting.IsActionReady)
                {
                    if (canSeeTarget)
                        TargetLastSeenPos = target.GridPos;

                    var targetPos = isFearful
                        ? CFearful.GetTargetRetreatPoint(ControlledThing.GridPos, ((CFearful)fearful).FearedThing.GridPos, ControlledThing.ContainingGridManager)
                        : TargetLastSeenPos;

                    //RoguemojiGame.Instance.DebugGridLine(GridPos, targetPos, canSeeTarget ? new Color(0f, 0f, 1f, 0.2f) : new Color(1f, 0f, 0f, 0.2f), 0.5f, CurrentLevelId);

                    TryToMoveToPos(targetPos);
                }

                if (!isFearful)
                {
                    if (canSeeTarget)
                    {
                        CantSeeTargetElapsedTime = 0f;
                    }
                    else
                    {
                        CantSeeTargetElapsedTime += dt;
                        if (CantSeeTargetElapsedTime > CantSeeTargetLoseDelay)
                            targeting.LoseTarget();
                    }
                }
            }
        }
    }

    public void TryToMoveToPos(IntVector gridPos)
    {
        var path = ControlledThing.GetPathTo(ControlledThing.GridPos, gridPos);
        if (path != null && path.Count > 0 && !path[0].Equals(ControlledThing.GridPos))
        {
            var dir = GridManager.GetDirectionForIntVector(path[0] - ControlledThing.GridPos);
            ControlledThing.TryMove(dir, out bool switchedLevel);
        }

        if (ControlledThing.GetComponent<CActing>(out var component))
        {
            var acting = (CActing)component;
            acting.PerformedAction();
        }
    }

    public override void OnTakeDamageFrom(Thing thing)
    {
        base.OnTakeDamageFrom(thing);

        int amount = thing.GetAttackDamage();
        if (amount > 0 && ControlledThing.GetStatClamped(StatType.Health) == 1 && !ControlledThing.HasComponent<CFearful>())
        {
            var fearful = ControlledThing.AddComponent<CFearful>();
            fearful.Lifetime = 5f;

            if (thing.GetComponent<CProjectile>(out var component))
                fearful.FearedThing = ((CProjectile)component).Thrower;
            else
                fearful.FearedThing = thing;
        }
    }

    public override void OnFindTarget(Thing target)
    {
        base.OnFindTarget(target);

        ControlledThing.RemoveFloater("❔");
        ControlledThing.AddFloater("❕", 1.55f, new Vector2(0f, -10f), new Vector2(0f, -35f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.QuadOut, 0.05f);

        if (ControlledThing.GetComponent<CActing>(out var component))
        {
            var acting = (CActing)component;
            acting.PerformedAction();
            acting.ActionTimer = Game.Random.Float(0f, 0.1f);
        }

        TargetLastSeenPos = target.GridPos;
    }

    public override void OnLoseTarget()
    {
        base.OnLoseTarget();

        ControlledThing.RemoveFloater("❕");
        ControlledThing.AddFloater("❔", Game.Random.Float(0.95f, 1.1f), new Vector2(0f, -10f), new Vector2(0f, -30f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.QuadOut, 0.1f);

        if (ControlledThing.GetComponent<CActing>(out var component))
        {
            var acting = (CActing)component;
            acting.PerformedAction();
            acting.ActionTimer = Game.Random.Float(0f, 0.1f);
        }

        WanderGridPos = TargetLastSeenPos;
    }
}