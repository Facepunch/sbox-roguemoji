using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class MAllergicNuts : Mutation
{
    public override void Init(Thing thing)
    {
        base.Init(thing);

        Trait = thing.AddTrait("Nut Allergy", "🥺", $"Eating nuts will poison you", offset: new Vector2(2f, 2f), tattooIcon: "🌰", tattooScale: 0.6f, tattooOffset: new Vector2(-15f, -15f));
    }

    public override void OnUseThing(Thing thing)
    {
        if(thing is Nut)
        {
            if (!Thing.HasStat(StatType.Health))
                return;

            var poison = Thing.AddComponent<CPoisoned>();
            poison.Lifetime = 60f;
            Thing.AddSideFloater(Globals.Icon(IconType.Poison));

            if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
                ((CIconPriority)component).AddIconPriority("🤢", (int)PlayerIconPriority.NutAllergyReaction, 2.0f);
        }
    }

    // todo: effect when holding nut or hit by nut
}