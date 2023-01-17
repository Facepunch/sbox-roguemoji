using Sandbox;
using System;

namespace Roguemoji;

public class VfxNudge : ThingComponent
{
    public Direction Direction { get; set; }
    public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientComponent = true;

        var dir = GridManager.GetVectorForDirection(Direction);
        Thing.SetMoveOffset(dir * 0f);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var dir = GridManager.GetVectorForDirection(Direction);
        Thing.SetMoveOffset(dir * Utils.MapReturn(TimeElapsed, 0f, Lifetime, 0f, Distance, EasingType.QuadOut));

        if(TimeElapsed > Lifetime)
            Remove();
    }

    public override void ReInitialize()
    {

    }

    public override void OnRemove()
    {
        Thing.SetMoveOffset(Vector2.Zero);
    }
}