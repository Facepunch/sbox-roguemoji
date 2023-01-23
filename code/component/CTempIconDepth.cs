using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CTempIconDepth : ThingComponent
{
    public float Lifetime { get; set; }
    private int _oldIconDepth = -1;

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;

        if(TimeElapsed > Lifetime)
            Remove();
    }

    public void SetTempIconDepth(int depth)
    {
        _oldIconDepth = Thing.IconDepth;
        Thing.IconDepth = depth;
    }

    public override void OnRemove()
    {
        if (_oldIconDepth >= 0)
            Thing.IconDepth = _oldIconDepth;
    }
}