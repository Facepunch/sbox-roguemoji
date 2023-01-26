using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionInvisible : Potion
{
    public override string AbilityName => "Quaff Potion";
    public override string SplashIcon => Globals.Icon(IconType.Invisible);

    public PotionInvisible()
    {
        PotionType = PotionType.Invisibility;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Makes drinker invisible";
        Tooltip = "An invisibility potion";
        
        SetTattoo(Globals.Icon(IconType.Invisible));

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "😋", $"Consume potion to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f), isAbility: true);
            AddTrait("", Globals.Icon(IconType.Invisible), $"Makes you invisible", offset: new Vector2(0f, 0f));
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
        MakeInvisible(thing);

        if (thing.WieldedThing != null)
            MakeInvisible(thing.WieldedThing);
    }

    void MakeInvisible(Thing thing)
    {
        var invisible = thing.AddComponent<CInvisible>();
        invisible.Lifetime = 610f;
    }
}
