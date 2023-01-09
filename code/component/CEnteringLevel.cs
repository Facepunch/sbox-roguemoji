using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CEnteringLevel : ThingComponent
{
    public float Lifetime { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        Lifetime = 0.75f;

        if(thing is RoguemojiPlayer player)
            player.VfxFadeCamera(lifetime: 0.74f, shouldFadeOut: false);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;

        if(TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.IsInTransit = false;
    }
}