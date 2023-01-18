using Sandbox;
using System;

namespace Roguemoji;

public class CActing : ThingComponent
{
    public float ActionTimer { get; set; }
    public float ActionDelay { get; set; }
    public bool IsActionReady { get; set; }

    public int NumPreventActionSources { get; private set; } // the number of things that want to stop this from acting, eg. Sleeping or Stunned

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        ActionTimer = 0f;
        IsActionReady = false;
        NumPreventActionSources = 0;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if(NumPreventActionSources == 0)
        {
            ActionTimer += dt;

            bool wasActionReady = IsActionReady;
            IsActionReady = (ActionTimer >= ActionDelay);

            if (IsActionReady && !wasActionReady)
                Thing.OnActionRecharged();

            Thing.ActionRechargePercent = Math.Clamp(ActionTimer / ActionDelay, 0f, 1f);
        }
        else
        {
            Thing.ActionRechargePercent = 0f;
        }
    }

    public void PerformedAction()
    {
        ActionTimer = 0f;
        IsActionReady = false;
    }

    public static float CalculateActionDelay(int speed)
    {
        return Utils.Map(speed, 0, 10, 1.0f, 0.1f);
    }

    public void PreventAction()
    {
        NumPreventActionSources++;
        IsActionReady = false;
        ActionTimer = 0f;
    }

    public void AllowAction()
    {
        NumPreventActionSources--;
    }
}