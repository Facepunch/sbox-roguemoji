using Sandbox;
using System;

namespace Roguemoji;

public class CActing : ThingComponent
{
    public float TimeElapsed { get; set; }
    public float ActionDelay { get; set; }
    public bool IsActionReady { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        TimeElapsed = 0f;
        IsActionReady = false;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;

        bool wasActionReady = IsActionReady;
        IsActionReady = (TimeElapsed >= ActionDelay);

        if (IsActionReady && !wasActionReady)
            Thing.OnActionRecharged();

        Thing.ActionRechargePercent = Math.Clamp(TimeElapsed / ActionDelay, 0f, 1f);
    }

    public void PerformedAction()
    {
        TimeElapsed = 0f;
        IsActionReady = false;
    }

    public static float CalculateActionDelay(int speed)
    {
        return Utils.Map(speed, 0, 10, 1.0f, 0.1f);
    }
}