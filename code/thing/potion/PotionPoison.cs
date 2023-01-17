using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionPoison : Potion
{
    public override string AbilityName => "Quaff Potion";
    public override string SplashIcon => Globals.Icon(IconType.Poison);
    public int HealthAmount { get; private set; }

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
            HealthAmount = 5;
            AddTrait(AbilityName, "😋", $"Consume potion to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f));
            AddTrait("", GetStatIcon(StatType.Health), $"Drinking recovers {HealthAmount}{GetStatIcon(StatType.Health)}", offset: new Vector2(0f, -1f), labelText: $"+{HealthAmount}", labelFontSize: 16, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
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
