using Sandbox;
using System;

namespace Roguemoji;

public class CompCooldown : ThingComponent
{
    public float CooldownDuration { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = false;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (Thing.IsOnCooldown)
        {
            Thing.CooldownTimer -= dt;
            if (Thing.CooldownTimer < 0f)
                FinishCooldown();
            else
                Thing.CooldownProgressPercent = Utils.Map(Thing.CooldownTimer, CooldownDuration, 0f, 0f, 1f);
        }
    }

    public void StartCooldown(float time)
    {
        ShouldUpdate = true;
        CooldownDuration = time;
        Thing.CooldownTimer = time;
        Thing.IsOnCooldown = true;
        Thing.CooldownProgressPercent = 0f;

        Thing.OnCooldownStart();
    }

    public void FinishCooldown()
    {
        ShouldUpdate = false;
        Thing.IsOnCooldown = false;
        Thing.OnCooldownFinish();
    }
}