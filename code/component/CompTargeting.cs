using Sandbox;
using System;

namespace Roguemoji;

public class CompTargeting : ThingComponent
{
    public Thing Target { get; set; }

    public bool HasTarget => Target != null;

    public void SetTarget(Thing target)
    {
        Target = target;
    }
}