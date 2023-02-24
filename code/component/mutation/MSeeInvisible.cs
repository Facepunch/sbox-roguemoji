using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class MSeeInvisible : Mutation
{
    public int PerceptionAmount { get; private set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        Trait = thing.AddTrait("Keen Eye", "🧐", $"Slightly improves ability to see invisible things", offset: Vector2.Zero, tattooIcon: Thing.GetStatIcon(StatType.SightDistance), tattooScale: 0.375f, tattooOffset: new Vector2(5.25f, -0.5f));

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("🧐", (int)PlayerIconPriority.GainMutation, 1.5f);

        PerceptionAmount = 1;

        if (!thing.HasStat(StatType.Perception))
            thing.InitStat(StatType.Perception, PerceptionAmount);
        else
            thing.AdjustStat(StatType.Perception, PerceptionAmount);
    }

    public override void OnRemove()
    {
        base.OnRemove();

        Thing.AdjustStat(StatType.Perception, -PerceptionAmount);
    }
}