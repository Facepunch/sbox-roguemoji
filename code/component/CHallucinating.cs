using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CHallucinating : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        if (Thing.Brain is RoguemojiPlayer player)
            player.HallucinatingSeed = Game.Random.Int(1, 999);

        Trait = thing.AddTrait("Hallucinating", Globals.Icon(IconType.Hallucination), $"Sees things in a new way", offset: Vector2.Zero);

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component))
            IconId = ((CIconPriority)component).AddIconPriority("🤪", (int)PlayerIconPriority.Hallucinating);

        thing.AddFloater(Globals.Icon(IconType.Hallucination), time: 0f, new Vector2(-14f, 4f), Vector2.Zero, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.025f, scale: 0.5f, opacity: 0.25f);
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
        if (Thing.Brain is RoguemojiPlayer player)
            player.HallucinatingSeed = 0;

        Thing.RemoveTrait(Trait);

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);

        Thing.RemoveFloater(Globals.Icon(IconType.Hallucination));
    }

    public override void OnThingDestroyed()
    {
        Thing.RemoveFloater(Globals.Icon(IconType.Hallucination));
    }

    public override void OnThingDied()
    {
        Remove();
    }
}