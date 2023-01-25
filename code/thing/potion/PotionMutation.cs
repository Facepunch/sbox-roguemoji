using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionMutation : Potion
{
    public override string AbilityName => "Quaff Potion";
    public override string SplashIcon => Globals.Icon(IconType.Mutation);

    public PotionMutation()
    {
        PotionType = PotionType.Mutation;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Causes unpredictable changes";
        Tooltip = "A mutation potion";
        
        SetTattoo(Globals.Icon(IconType.Mutation));

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "😋", $"Consume potion to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f), isAbility: true);
            AddTrait("", Globals.Icon(IconType.Mutation), $"Adds a positive or negative trait", offset: new Vector2(0f, 0f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        return true;
    }

    public override void Use(Thing user)
    {
        ApplyEffectToThing(user);
        Destroy();

        base.Use(user);
    }

    public override void ApplyEffectToThing(Thing thing)
    {
        //thing.AddSideFloater(Globals.Icon(IconType.Mutation));
    }
}
