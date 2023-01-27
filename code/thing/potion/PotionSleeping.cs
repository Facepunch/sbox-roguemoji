using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionSleeping : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Sleeping);

    public PotionSleeping()
    {
        PotionType = PotionType.Sleeping;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Puts drinker to sleep";
        Tooltip = "A sleeping potion";
        
        SetTattoo(Globals.Icon(IconType.Sleeping));

        if (Game.IsServer)
        {
            AddTrait("", "😴", $"Puts you to sleep", offset: new Vector2(0f, 0f));
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

        var sleeping = thing.AddComponent<CSleeping>();
        sleeping.Lifetime = 20f;
        thing.AddSideFloater(Globals.Icon(IconType.Sleeping));
    }
}
