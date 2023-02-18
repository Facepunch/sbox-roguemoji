using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class MTeleportitis : ThingComponent
{
    public Trait Trait { get; private set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        Trait = thing.AddTrait("Teleportitis", "😯", $"You might teleport when hit", offset: Vector2.Zero, tattooIcon: "➰", tattooScale: 0.6f, tattooOffset: new Vector2(0f, -12f));
    }

    public override void OnRemove()
    {
        Thing.RemoveTrait(Trait);
    }

    public override void OnThingDied()
    {
        Remove();
    }

    public override void OnBumpedIntoBy(Thing thing, Direction direction)
    {
        if(Game.Random.Int(0, 5) == 0)
        {
            ScrollTeleport.TeleportThing(Thing);
        }
    }
}