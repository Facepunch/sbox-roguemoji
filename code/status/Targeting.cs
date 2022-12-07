using Sandbox;
using System;

namespace Roguemoji;

public class Targeting : ThingComponent
{
    public Thing Target { get; set; }

    public void SetTarget(Thing target)
    {
        Target = target;
    }
}