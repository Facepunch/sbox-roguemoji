using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class Mutation : ThingComponent
{
    public Trait Trait { get; protected set; }

    public override void OnRemove()
    {
        Thing.RemoveTrait(Trait);
    }

    public override void OnThingDied()
    {
        Remove();
    }
}