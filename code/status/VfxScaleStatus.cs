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
    }

    public override void Update(float dt)
    {
        //Log.Info("update: " + TimeSinceStart + " / " + Lifetime + " start: " + StartScale + " end: " + EndScale);
        //Thing.SetFontSize(Thing.DefaultSize * Utils.Map(TimeSinceStart, 0f, Lifetime, StartScale, EndScale, EasingType.Linear));
        Thing.SetFontSize(10f);

        //Thing.DrawDebugText("" + Thing.FontSize);

        if(TimeSinceStart > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetFontSize(Thing.DefaultFontSize * EndScale);
    }
}