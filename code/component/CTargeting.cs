using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CTargeting : ThingComponent
{
    public Thing Target { get; set; }

    public bool HasTarget => Target != null;

    public FactionType TargetFaction { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        TargetFaction = FactionType.Player;
    }

    public void SetTarget(Thing target)
    {
        Target = target;
        Thing.OnFindTarget(target);
    }

    public void LoseTarget()
    {
        if(Target != null)
        {
            Target = null;
            Thing.OnLoseTarget();
        }
    }

    public override void OnChangedGridPos()
    {
        CheckForTargets();
    }

    public override void OnPlayerChangedGridPos(RoguemojiPlayer player)
    {
        if (Thing.Brain == player)
            return;

        if (EvaluateTarget(player.ControlledThing))
            SetTarget(player.ControlledThing);
    }

    public bool EvaluateTarget(Thing other)
    {
        if (!IsAppropriateTarget(other))
            return false;

        int distance = Utils.GetDistance(Thing.GridPos, other.GridPos);
        if (Target == null || distance < Utils.GetDistance(Thing.GridPos, Target.GridPos))
        {
            return Thing.CanSeeThing(other);
        }

        return false;
    }

    public bool IsAppropriateTarget(Thing other)
    {
        return other.Faction == TargetFaction && other != Target;
    }

    private List<Thing> _potentialTargets = new List<Thing>();
    public void CheckForTargets()
    {
        var gridManager = Thing.ContainingGridManager;
        _potentialTargets.Clear();
        foreach(var pair in gridManager.GridThings)
        {
            var things = pair.Value;
            foreach(var other in things)
            {
                if (other == null || other == Thing || other.IsRemoved || other.IsInTransit)
                    continue;

                if(other.Faction == TargetFaction)
                    _potentialTargets.Add(other);
            }
        }

        int closestDistance = int.MaxValue;
        Thing target = null;

        foreach (var other in _potentialTargets)
        {
            if(Thing.CanSeeThing(other))
            {
                target = other;

                var distance = Utils.GetDistance(Thing.GridPos, other.GridPos);
                if(distance < closestDistance)
                {
                    target = other;
                    closestDistance = distance;
                }
            }
        }

        if(target != null && target != Target)
        {
            SetTarget(target);
        }
    }
}