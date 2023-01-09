using Sandbox;
using System;

namespace Roguemoji;

public class VfxSpin : ThingComponent
{
    public float Lifetime { get; set; }
    public float StartAngle { get; set; }
    public float EndAngle { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientComponent = true;
        Thing.SetRotation(StartAngle);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        Thing.SetRotation(Utils.Map(TimeElapsed, 0f, Lifetime, StartAngle, EndAngle, EasingType.QuadOut));

        if (TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetRotation(EndAngle);
    }
}