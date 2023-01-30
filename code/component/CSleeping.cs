using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CSleeping : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }
    public int IconId { get; set; }

    // todo: loud noises nearby should wake thing up

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        Trait = thing.AddTrait("Sleeping", Globals.Icon(IconType.Sleeping), $"Napping until rested or hit", offset: Vector2.Zero);

        if (thing.GetComponent<CActing>(out var component))
            ((CActing)component).PreventAction();

        if (thing is RoguemojiPlayer && thing.GetComponent<CIconPriority>(out var component2))
            IconId = ((CIconPriority)component2).AddIconPriority("😴", (int)PlayerIconPriority.Sleeping);

        thing.AddFloater(Globals.Icon(IconType.Sleeping), time: 0f, new Vector2(15f, -10f), Vector2.Zero, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.025f, scale: 0.5f, opacity: 0.66f);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;
        if(Lifetime > 0f && TimeElapsed > Lifetime)
        {
            Remove();
            return;
        }

        Trait.BarPercent = 1f - Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f);
    }

    public override void OnRemove()
    {
        Thing.RemoveTrait(Trait);

        if (Thing is RoguemojiPlayer player)
            player.ClearQueuedAction();
            
        if (Thing.GetComponent<CActing>(out var component))
            ((CActing)component).AllowAction();

        if (Thing is RoguemojiPlayer && Thing.GetComponent<CIconPriority>(out var component2))
            ((CIconPriority)component2).RemoveIconPriority(IconId);

        Thing.RemoveFloater(Globals.Icon(IconType.Sleeping));
    }

    public override void OnThingDestroyed()
    {
        Thing.RemoveFloater(Globals.Icon(IconType.Sleeping));
    }

    public override void OnThingDied()
    {
        Remove();
    }

    public override void OnBumpedIntoBy(Thing thing)
    {
        if(TimeElapsed > 0f)
            Remove();
    }
}