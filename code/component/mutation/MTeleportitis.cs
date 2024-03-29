﻿using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class MTeleportitis : Mutation
{
    public override void Init(Thing thing)
    {
        base.Init(thing);

        Trait = thing.AddTrait("Teleportitis", "😯", $"You might teleport when hit", offset: Vector2.Zero, tattooIcon: "➰", tattooScale: 0.6f, tattooOffset: new Vector2(0f, -12f));

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😯", (int)PlayerIconPriority.GainMutation, 1.5f);
    }

    public override void OnBumpedIntoBy(Thing thing, Direction direction)
    {
        if(Game.Random.Int(0, 5) == 0)
        {
            ScrollTeleport.TeleportThing(Thing);
        }
    }
}