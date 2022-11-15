using Sandbox;
using System;

namespace Interfacer;

public class VfxShakeStatus : ThingStatus
{
    public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
    }

    public override void Update(float dt)
    {
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