using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CHallucinating : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        if (Thing is RoguemojiPlayer player)
            player.HallucinatingSeed = Game.Random.Int(1, 999);

        Trait = thing.AddTrait("Hallucinating", Globals.Icon(IconType.Hallucination), $"Sees things in a new way", offset: Vector2.Zero);

        RoguemojiGame.Instance.AddFloater(Globals.Icon(IconType.Hallucination), Thing.GridPos, time: 0f, Thing.CurrentLevelId, new Vector2(-14f, 4f), Vector2.Zero, height: 0f, text: "", requireSight: true, EasingType.Linear, fadeInTime: 0.025f, scale: 0.5f, opacity: 0.25f, parent: Thing);
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
        if (Thing is RoguemojiPlayer player)
            player.HallucinatingSeed = 0;

        Thing.RemoveTrait(Trait);
        RoguemojiGame.Instance.RemoveFloater(Globals.Icon(IconType.Hallucination), Thing.CurrentLevelId, parent: Thing);
    }

    public override void OnThingDestroyed()
    {
        RoguemojiGame.Instance.RemoveFloater(Globals.Icon(IconType.Hallucination), Thing.CurrentLevelId, parent: Thing);
    }

    public override void OnThingDied()
    {
        Remove();
    }
}