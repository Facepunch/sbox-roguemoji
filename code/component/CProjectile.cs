using Sandbox;
using System;

namespace Roguemoji;

public class CProjectile : ThingComponent
{
    public Direction Direction { get; set; }
    public float MoveDelay { get; set; }
    public TimeSince TimeSinceMove { get; set; }
    public int RemainingDistance { get; set; }
    public Thing Thrower { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        TimeSinceMove = 0f;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if(Thing.ContainingGridType != GridType.Arena)
        {
            Remove();
            return;
        }

        if(TimeSinceMove > MoveDelay)
        {
            TimeSinceMove = 0f;

            if (Thing.TryMove(Direction, shouldAnimate: false))
            {
                RemainingDistance--;
                if (RemainingDistance <= 0)
                    Remove();
            }
        }

        //Thing.DebugText = $"{RemainingDistance}";
    }

    public override void OnBumpedIntoThing(Thing thing)
    {
        Remove();
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        Thing.DamageOther(thing, Direction);
        Remove();
    }

    public override void OnBumpedOutOfBounds(Direction dir)
    {
        Remove();
    }
}