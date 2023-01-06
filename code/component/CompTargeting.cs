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
        //Log.Info("OnChangedGridPos");
        CheckForTargets();
    }

    public override void OnPlayerChangedGridPos(RoguemojiPlayer player)
    {
        if (Thing == player)
            return;

        if (EvaluateTarget(player))
            SetTarget(player);
    }

    public bool EvaluateTarget(Thing thing)
    {
        if (thing == null || thing.Faction != TargetFaction || thing.IsRemoved || thing == Target)
            return false;

        int sight = Thing.GetStatClamped(StatType.Sight);
        int distance = Utils.GetDistance(Thing.GridPos, thing.GridPos);
        int existingDistance = Target != null ? Utils.GetDistance(Thing.GridPos, Target.GridPos) : int.MaxValue;
        if (distance <= sight && distance < existingDistance)
        {
            if (Thing.ContainingGridManager.HasLineOfSight(Thing.GridPos, thing.GridPos, sight, out IntVector collisionCell))
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
            foreach(var thing in things)
            {
                if (thing == null || thing == Thing || thing.IsRemoved)
                    continue;

                if(thing.Faction == TargetFaction)
                    _potentialTargets.Add(thing);
            }
        }

        int closestDistance = int.MaxValue;
        Thing target = null;
        int sight = Thing.GetStatClamped(StatType.Sight);

        foreach (var thing in _potentialTargets)
        {
            int distance = Utils.GetDistance(Thing.GridPos, thing.GridPos);
            if(distance <= sight && distance < closestDistance)
            {
                if(gridManager.HasLineOfSight(Thing.GridPos, thing.GridPos, sight, out IntVector collisionCell))
                {
                    target = thing;
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