using Sandbox;
using System;

namespace Roguemoji;

public class CProjectile : ThingComponent
{
    public Direction Direction { get; set; }
    public float MoveDelay { get; set; }
    public TimeSince TimeSinceMove { get; set; }
    public int CurrentNumMoves { get; set; }
    public int TotalDistance { get; set; }
    public Thing Thrower { get; set; }
    public bool ShouldHit { get; set; }
    public Vector2 DirectionVector { get; private set; }
    public bool ShouldUseDirectionVector { get; private set; }
    public Vector2 WorldPos { get; private set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        TimeSinceMove = 0f;
        ShouldHit = true;

        if (thing.GetComponent<CActing>(out var component))
            ((CActing)component).PreventAction();
    }

    public override void ReInitialize()
    {
        base.ReInitialize();

        CurrentNumMoves = 0;
        TimeSinceMove = 0f;
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
            CurrentNumMoves++;
            TimeSinceMove = 0f;

            if(ShouldUseDirectionVector)
            {
                WorldPos += DirectionVector * RoguemojiGame.CellSize;
                IntVector newGridPos = Thing.ContainingGridManager.GetGridPosForWorldPos(WorldPos);
                if(!newGridPos.Equals(Thing.GridPos))
                {
                    var direction = GridManager.GetDirectionForIntVector(newGridPos - Thing.GridPos);
                    if (Thing.TryMove(direction, out bool switchedLevel, out bool actionWasntReady, shouldAnimate: true, dontRequireAction: true))
                    {
                        if (CurrentNumMoves >= TotalDistance)
                            Remove();
                    }
                }
            }
            else
            {
                if (Thing.TryMove(Direction, out bool switchedLevel, out bool actionWasntReady, shouldAnimate: true, dontRequireAction: true))
                {
                    if (CurrentNumMoves >= TotalDistance)
                        Remove();
                }
            }
        }

        //Thing.DebugText = $"{RemainingDistance}, {MoveDelay}, {TimeSinceMove}";
    }

    public override void OnBumpedIntoThing(Thing thing, Direction direction)
    {
        if(TimeElapsed > 0f)
            Remove();
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        if (!ShouldHit)
            return;

        if(TimeElapsed > 0f && thing.HasFlag(ThingFlags.Solid))
            Thing.HitOther(thing, Direction);

        Remove();
    }

    public override void OnBumpedOutOfBounds(Direction direction)
    {
        if (TimeElapsed > 0f)
            Remove();
    }

    public override void OnRemove()
    {
        if (Thing.GetComponent<CActing>(out var component))
            ((CActing)component).AllowAction();
    }

    public void UseDirectionVector(Vector2 vec)
    {
        ShouldUseDirectionVector = true;
        DirectionVector = vec;
        WorldPos = Thing.ContainingGridManager.GetWorldPosForGridPos(Thing.GridPos);
    }
}