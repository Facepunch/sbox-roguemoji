using Sandbox;
using System;

namespace Interfacer;

public class VfxScaleStatus : ThingStatus
{
    public float Lifetime { get; set; }
    public float StartScale { get; set; }
    public float EndScale { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        Thing.SetScale(StartScale);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        //Log.Info("update: " + TimeSinceStart + " / " + Lifetime + " start: " + StartScale + " end: " + EndScale);
        Thing.SetScale(Utils.Map(TimeSinceStart, 0f, Lifetime, StartScale, EndScale, EasingType.Linear));

        //Thing.DrawDebugText("" + Thing.IconScale);

        if(TimeSinceStart > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetScale(EndScale);
    }
}