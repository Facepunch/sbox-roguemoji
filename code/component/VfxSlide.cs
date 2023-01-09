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
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var dir = GridManager.GetVectorForDirection(Direction);
        Thing.SetOffset(dir * Utils.Map(TimeElapsed, 0f, Lifetime, -Distance, 0f, EasingType.ExpoOut));

        if(TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetOffset(Vector2.Zero);
    }
}