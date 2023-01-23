using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionMedicine : Potion
{
    public override string AbilityName => "Quaff Potion";
    public override string SplashIcon => Globals.Icon(IconType.Medicine);

    public PotionMedicine()
    {
        PotionType = PotionType.Confusion;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Cures ailments";
        Tooltip = "A medicine potion";
        
        SetTattoo(Globals.Icon(IconType.Medicine));

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "😋", $"Consume potion to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f), isAbility: true);
            AddTrait("", Globals.Icon(IconType.Medicine), $"Cures {Globals.Icon(IconType.Poison)}{Globals.Icon(IconType.Hallucination)}{Globals.Icon(IconType.Confusion)}{Globals.Icon(IconType.Blindness)}{Globals.Icon(IconType.Fear)}", offset: new Vector2(0f, 0f));
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
        if (!thing.HasComponent<CActing>())
            return;

        thing.RemoveComponent<CPoisoned>();
        thing.RemoveComponent<CHallucinating>();
        thing.RemoveComponent<CConfused>();
        thing.RemoveComponent<CBlinded>();
        thing.RemoveComponent<CFearful>();

        thing.AddSideFloater(Globals.Icon(IconType.Medicine));
    }
}
