using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionAmnesia : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Amnesia);

    public PotionAmnesia()
    {
        PotionType = PotionType.Amnesia;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Forget knowledge of places and things";
        Tooltip = "An amnesia potion";
        
        SetTattoo(Globals.Icon(IconType.Amnesia));

        if (Game.IsServer)
        {
            AddTrait("", Globals.Icon(IconType.Amnesia), $"Forget everything you know", offset: new Vector2(0f, 0f));
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
        if (thing.Brain is RoguemojiPlayer player)
        {
            player.ResetScrollKnowledge();
            player.ResetPotionKnowledge(); 
            player.ClearVisionKnowledgeClient();
        }

        if (thing.GetComponent<CTargeting>(out var component))
            ((CTargeting)component).LoseTarget();
    }
}
