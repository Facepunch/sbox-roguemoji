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
        switch(speed)
        {
            case 0: return 2.0f;
            case 1: return 1.7f;
            case 2: return 1.5f;
            case 3: return 1.2f;
            case 4: return 1.0f;
            case 5: return 0.95f;
            case 6: return 0.9f;
            case 7: return 0.85f;
            case 8: return 0.8f;
            case 9: return 0.75f;
            case 10: return 0.7f;
            case 11: return 0.65f;
            case 12: return 0.6f;
            case 13: return 0.55f;
            case 14: return 0.5f;
            case 15: return 0.45f;
            case 16: return 0.4f;
            case 17: return 0.35f;
            case 18: return 0.3f;
            case 19: return 0.29f;
            case 20: return 0.28f;
            case 21: return 0.27f;
            case 22: return 0.26f;
            case 23: return 0.25f;
            case 24: return 0.24f;
            case 25: return 0.23f;
            case 26: return 0.22f;
            case 27: return 0.21f;
            case 28: return 0.20f;
            case 29: return 0.19f;
            case 30: return 0.18f;
            case 31: return 0.17f;
            case 32: return 0.16f;
            case 33: default:  return 0.15f;
        }
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