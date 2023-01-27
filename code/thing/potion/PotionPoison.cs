using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionPoison : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Poison);

    public PotionPoison()
    {
        PotionType = PotionType.Poison;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Poisons the drinker";
        Tooltip = "A poison potion";
        
        SetTattoo(Globals.Icon(IconType.Poison));

        if (Game.IsServer)
        {
            AddTrait("", "🤒", $"Poisons the drinker, causing damage over time", offset: new Vector2(0f, 0f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (!user.HasStat(StatType.Health))
            return false;

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
        if (!thing.HasStat(StatType.Health))
            return;

        var poison = thing.AddComponent<CPoisoned>();
        poison.Lifetime = 60f;
        thing.AddSideFloater(Globals.Icon(IconType.Poison));
    }
}
