using Sandbox;
using System;

namespace Roguemoji;

public class CProjectile : ThingComponent
{
    public Direction Direction { get; set; }
    public float MoveDelay { get; set; }
    public TimeSince TimeSinceMove { get; set; }
    public int CurrentDistance { get; set; }
    public int TotalDistance { get; set; }
    public Thing Thrower { get; set; }
    public bool ShouldHit { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        TimeSinceMove = 0f;
        ShouldHit = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (Thing.ContainingGridType != GridType.Arena)
        {
            Remove();
            return;
        }

        if(TimeSinceMove > MoveDelay)
        {
            TimeSinceMove = 0f;

            if (Thing.TryMove(Direction, shouldAnimate: false))
            {
                CurrentDistance++;
                if (CurrentDistance >= TotalDistance)
                    Remove();
            }
        }

        //Thing.DebugText = $"{RemainingDistance}, {MoveDelay}, {TimeSinceMove}";
    }

    public override void OnBumpedIntoThing(Thing thing)
    {
        if(TimeElapsed > 0f)
            Remove();
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        if(ShouldHit && TimeElapsed > 0f && thing.HasFlag(ThingFlags.Solid))
            Thing.HitOther(thing, Direction);

        Remove();
    }

    public override void OnBumpedOutOfBounds(Direction dir)
    {
        if (TimeElapsed > 0f)
            Remove();
    }

    public override void OnRemove()
    {
        base.OnRemove();
    }
}