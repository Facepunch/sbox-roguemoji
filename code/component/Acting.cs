using Sandbox;
using System;

namespace Roguemoji;

public class Acting : ThingComponent
{
    public TimeSince TimeSinceAction { get; set; }
    public float ActionDelay { get; set; }
    public bool IsActionReady { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        TimeSinceAction = 0f;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        bool wasInputReady = IsActionReady;
        IsActionReady = TimeSinceAction >= ActionDelay;

        if (IsActionReady && !wasInputReady)
            Thing.OnActionRecharged();

        Thing.ActionRechargePercent = Math.Clamp(TimeSinceAction / ActionDelay, 0f, 1f);
    }

    public void PerformedAction()
    {
        TimeSinceAction = 0f;
        IsActionReady = false;
    }
}