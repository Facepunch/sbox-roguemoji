using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CInvisible : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        Trait = thing.AddTrait("Invisible", Globals.Icon(IconType.Invisible), $"Can't be seen by most things/Hitting or getting hit ends effect", offset: Vector2.Zero);

        if (!thing.HasStat(StatType.Invisible))
            thing.InitStat(StatType.Invisible, 1);
        else
            thing.AdjustStat(StatType.Invisible, 1);

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component))
            IconId = ((CIconPriority)component).AddIconPriority("🤫", (int)PlayerIconPriority.Invisible);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if(Lifetime > 0f)
        {
            TimeElapsed += dt;
            if (TimeElapsed > Lifetime)
            {
                Remove();
                return;
            }

            Trait.BarPercent = 1f - Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f);
        }
    }

    public override void OnRemove()
    {
        Thing.RemoveTrait(Trait);

        Thing.AdjustStat(StatType.Invisible, -1);

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);
    }

    public override void OnBumpedIntoThing(Thing thing, Direction direction)
    {
        if (TimeElapsed > 0f)
            Remove();
    }

    public override void OnBumpedIntoBy(Thing thing, Direction direction)
    {
        if (TimeElapsed > 0f)
            Remove();
    }

    public override void OnThingDied()
    {
        Remove();
    }
}