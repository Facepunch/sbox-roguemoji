using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CConfused : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        Trait = thing.AddTrait("Confused", Globals.Icon(IconType.Confusion), $"Confused and woozy", offset: Vector2.Zero);
        RoguemojiGame.Instance.AddFloater(Globals.Icon(IconType.Confusion), Thing.GridPos, time: 0f, Thing.CurrentLevelId, new Vector2(15f, -16f), Vector2.Zero, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.025f, scale: 0.4f, opacity: 0.4f, parent: Thing);

        if(Thing is RoguemojiPlayer player)
            player.ConfusionSeed = Game.Random.Int(1, 999);

        if (thing is RoguemojiPlayer && thing.GetComponent<CIconPriority>(out var component))
            IconId = ((CIconPriority)component).AddIconPriority("😵", (int)PlayerIconPriority.Confused);
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
        RoguemojiGame.Instance.RemoveFloater(Globals.Icon(IconType.Confusion), Thing.CurrentLevelId, parent: Thing);

        if (Thing is RoguemojiPlayer player)
            player.ConfusionSeed = 0;

        if (Thing is RoguemojiPlayer && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);
    }

    public override void OnThingDestroyed()
    {
        RoguemojiGame.Instance.RemoveFloater(Globals.Icon(IconType.Confusion), Thing.CurrentLevelId, parent: Thing);
    }

    public override void OnThingDied()
    {
        Remove();
    }
}