using Sandbox;
using System;

namespace Roguemoji;

public class VfxShake : ThingComponent
{
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

        var dir = Utils.DegreesToVector(Rand.Float(0f, 360f));
        Thing.SetOffset(dir * Utils.Map(TimeSinceStart, 0f, Lifetime, Distance, 0f, EasingType.QuadOut));

        if(TimeSinceStart > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetOffset(Vector2.Zero);
    }
}