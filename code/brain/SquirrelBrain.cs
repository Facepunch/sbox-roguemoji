using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class SquirrelBrain : ThingBrain
{
    public float CantSeeTargetElapsedTime { get; private set; }
    public float CantSeeTargetLoseDelay { get; private set; }
    public IntVector TargetLastKnownPos { get; set; }
    public IntVector WanderGridPos { get; set; }

    public SquirrelBrain()
    {
        WanderGridPos = new IntVector(0, 0);
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

        CActing acting = null;
        if (ControlledThing.GetComponent<CActing>(out var component))
            acting = (CActing)component;

        CTargeting targeting = null;
        if (ControlledThing.GetComponent<CTargeting>(out var component2))
            targeting = (CTargeting)component2;

        if (acting == null || targeting == null || ControlledThing.IsInTransit || ControlledThing.IsRemoved)
            return;

        ControlledThing.DebugText = $"Posi: {ControlledThing.GridPos}\nWand: {WanderGridPos}\nLast: {TargetLastKnownPos}\ntarget: {(targeting.HasTarget ? targeting.Target.DisplayName : "NONE")}";

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

            if (target == null || !target.IsValid)
            {
                targeting.LoseTarget();
                WanderGridPos = ControlledThing.GridPos;
                return;
            }

            if (target.CurrentLevelId != ControlledThing.CurrentLevelId)
            {
                targeting.LoseTarget();
            }
            else
            {
                bool canSeeTarget = ControlledThing.CanSeeThing(target);
                bool isFearful = ControlledThing.GetComponent<CFearful>(out var fearful);

                //RoguemojiGame.Instance.DebugGridLine(ControlledThing.GridPos, targeting.Target.GridPos, canSeeTarget ? new Color(0f, 0f, 1f, 0.8f) : new Color(1f, 0f, 0f, 0.8f), 0.025f);

                if (canSeeTarget)
                    TargetLastKnownPos = target.GridPos;

                if (acting.IsActionReady)
                {
                    var targetPos = isFearful
                        ? CFearful.GetTargetRetreatPoint(ControlledThing.GridPos, ((CFearful)fearful).FearedThing.GridPos, ControlledThing.ContainingGridManager)
                        : TargetLastKnownPos;

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
        CTargeting targeting = null;
        if (ControlledThing.GetComponent<CTargeting>(out var component2))
            targeting = (CTargeting)component2;



        var path = ControlledThing.GetPathTo(ControlledThing.GridPos, gridPos);
        if (path != null && path.Count > 0 && !path[0].Equals(ControlledThing.GridPos))
        {
            if(path.Count > 1)
            {
                var color = Color.White;
                if(targeting.HasTarget)
                {
                    var target = targeting.Target;
                    bool canSeeTarget = ControlledThing.CanSeeThing(target);

                    if (canSeeTarget)
                    {
                        color = new Color(1f, 0.3f, 0f, 0.7f);
                    }
                    else
                    {
                        color = new Color(1f, 0.3f, 0.6f, 0.7f);

                        RoguemojiGame.Instance.DebugGridLine(ControlledThing.GridPos, TargetLastKnownPos, new Color(1f, 0.6f, 1f, 0.5f), 0.5f);
                    }
                }
                else
                {
                    color = new Color(0.3f, 0.3f, 1f, 0.7f);

                    RoguemojiGame.Instance.DebugGridLine(ControlledThing.GridPos, WanderGridPos, new Color(0.2f, 0.2f, 1f, 0.5f), 0.5f);
                }

                Thing.DrawPath(path, color, 0.7f);
            }
            else
            {
                RoguemojiGame.Instance.DebugGridLine(ControlledThing.GridPos, path[0], Color.Red, 0.5f);
            }

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
        var health = ControlledThing.GetStatClamped(StatType.Health);
        if (amount > 0 && (health - amount) == 1 && !ControlledThing.HasComponent<CFearful>())
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

        TargetLastKnownPos = target.GridPos;
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

        WanderGridPos = TargetLastKnownPos;
    }

    public override void HearSound(string name, IntVector soundPos, Thing sourceThing, int loudness = 0, float volume = 1, float pitch = 1, bool noFalloff = false)
    {
        if (sourceThing == ControlledThing)
            return;

        CTargeting targeting = null;
        if (ControlledThing.GetComponent<CTargeting>(out var component))
            targeting = (CTargeting)component;

        if (targeting == null)
            return;

        if(!targeting.HasTarget)
        {
            if(WanderGridPos.Equals(ControlledThing.GridPos))
            {
                ControlledThing.AddFloater("❔", Game.Random.Float(0.7f, 0.8f), new Vector2(0f, -10f), new Vector2(0f, -30f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.QuadOut, 0.1f);
            }

            WanderGridPos = soundPos;
        }
        //else if (!ControlledThing.CanSeeThing(targeting.Target))
        //{
        //    TargetLastKnownPos = soundPos;
        //}
    }
}