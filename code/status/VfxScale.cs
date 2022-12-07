using Sandbox;
using System;

namespace Roguemoji;

public class VfxScale : ThingComponent
{
    public float Lifetime { get; set; }
    public float StartScale { get; set; }
    public float EndScale { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientComponent = true;
        Thing.SetScale(StartScale);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        Thing.SetScale(Utils.Map(TimeSinceStart, 0f, Lifetime, StartScale, EndScale, EasingType.Linear));

        if(TimeSinceStart > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetScale(EndScale);
    }
}