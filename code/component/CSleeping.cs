using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CSleeping : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        Trait = thing.AddTrait("Sleeping", "😴", $"Napping until rested or until something disturbs", offset: Vector2.Zero);

        if (thing.GetComponent<CActing>(out var component))
            ((CActing)component).PreventAction();

        RoguemojiGame.Instance.AddFloater(Globals.Icon(IconType.Sleeping), Thing.GridPos, time: 0f, Thing.CurrentLevelId, new Vector2(15f, -8f), Vector2.Zero, height: 0f, text: "", requireSight: true, EasingType.Linear, fadeInTime: 0.025f, scale: 0.5f, opacity: 0.66f, parent: Thing);
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

        RoguemojiGame.Instance.RemoveFloater(Globals.Icon(IconType.Sleeping), Thing.CurrentLevelId, parent: Thing);
    }

    public override void OnThingDestroyed()
    {
        RoguemojiGame.Instance.RemoveFloater(Globals.Icon(IconType.Sleeping), Thing.CurrentLevelId, parent: Thing);
    }

    public override void OnThingDied()
    {
        Remove();
    }
}