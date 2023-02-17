using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionBurning : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Burning);

    public PotionBurning()
    {
        PotionType = PotionType.Burning;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Ignites the drinker";
        Tooltip = "A burning potion";
        
        SetTattoo(Globals.Icon(IconType.Burning));

        if (Game.IsServer)
        {
            AddTrait("", Globals.Icon(IconType.Burning), $"Lights things on fire", offset: new Vector2(0f, 0f));
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
        if(!thing.ContainingGridManager.ShouldCellPutOutFire(thing.GridPos) && thing.Flammability > 0)
        {
            var burning = thing.AddComponent<CBurning>();
            burning.Lifetime = 30f;
        }
    }
}
