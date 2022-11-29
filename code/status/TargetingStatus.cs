using Sandbox;
using System;

namespace Interfacer;

public class TargetingStatus : ThingStatus
{
    public Thing Target { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);
    }

    public void SetTarget(Thing target)
    {
        Target = target;
    }
}