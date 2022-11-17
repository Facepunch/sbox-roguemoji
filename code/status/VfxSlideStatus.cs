using Sandbox;
using System;

namespace Interfacer;

public class VfxSlideStatus : ThingStatus
{
    public Direction Direction { get; set; }
    public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientStatus = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var dir = GridManager.GetVectorForDirection(Direction);
        Thing.SetOffset(dir * Utils.Map(TimeSinceStart, 0f, Lifetime, -Distance, 0f, EasingType.ExpoOut));

        if(TimeSinceStart > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetOffset(Vector2.Zero);
    }
}