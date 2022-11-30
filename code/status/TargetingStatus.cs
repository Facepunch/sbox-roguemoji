using Sandbox;
using System;

namespace Interfacer;

public class TargetingStatus : ThingStatus
{
    public Thing Target { get; set; }

    public void SetTarget(Thing target)
    {
        Target = target;
    }
}