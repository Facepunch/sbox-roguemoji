using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CompTargeting : ThingComponent
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
        if (Thing == player)
            return;

        if (EvaluateTarget(player))
            SetTarget(player);
    }

    public bool EvaluateTarget(Thing other)
    {
        if (other == null || other.Faction != TargetFaction || other.IsRemoved || other == Target)
            return false;

        int adjustedSight = Math.Max(Thing.GetStatClamped(StatType.Sight) - other.GetStatClamped(StatType.Stealth), 1);
        int distance = Utils.GetDistance(Thing.GridPos, other.GridPos);
        int existingDistance = Target != null ? Utils.GetDistance(Thing.GridPos, Target.GridPos) : int.MaxValue;
        if (distance <= adjustedSight && distance < existingDistance)
        {
            if (Thing.ContainingGridManager.HasLineOfSight(Thing.GridPos, other.GridPos, adjustedSight, out IntVector collisionCell))
                return true;
        }

        return false;
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
                if (other == null || other == Thing || other.IsRemoved)
                    continue;

                if(other.Faction == TargetFaction)
                    _potentialTargets.Add(other);
            }
        }

        int closestDistance = int.MaxValue;
        Thing target = null;
        int sight = Thing.GetStatClamped(StatType.Sight);

        foreach (var other in _potentialTargets)
        {
            int adjustedSight = Math.Max(sight - other.GetStatClamped(StatType.Stealth), 1);
            int distance = Utils.GetDistance(Thing.GridPos, other.GridPos);
            if (distance <= adjustedSight && distance < closestDistance)
            {
                if(gridManager.HasLineOfSight(Thing.GridPos, other.GridPos, adjustedSight, out IntVector collisionCell))
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