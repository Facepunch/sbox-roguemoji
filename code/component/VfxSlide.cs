using Sandbox;
using System;

namespace Roguemoji;

public class VfxSlide : ThingComponent
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
        Thing.SetMoveOffset(dir * -Distance);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var dir = GridManager.GetVectorForDirection(Direction);
        Thing.SetMoveOffset(dir * Utils.Map(TimeElapsed, 0f, Lifetime, -Distance, 0f, EasingType.QuadOut));

        if(TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetMoveOffset(Vector2.Zero);
    }
}