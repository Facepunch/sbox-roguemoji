using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class Refreshment : Thing
{
    public override string AbilityName => "Drink";

    public Refreshment()
	{
		DisplayIcon = "🥤";
        DisplayName = "Refreshment";
        Description = "Refreshes your item cooldowns";
        Tooltip = "A refreshment";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;
        Flammability = 13;

        if (Game.IsServer)
        {   
            AddTrait(AbilityName, "😋", $"Consume refreshment to refresh all item cooldowns", offset: new Vector2(0f, -1f), tattooIcon: "🥤", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f), isAbility: true);
        }
    }

    public override void Use(Thing user)
    {
        Destroy();

        if(user.Brain is RoguemojiPlayer player)
        {
            foreach(var thing in player.InventoryGridManager.GetAllThings())
            {
                if (thing.IsOnCooldown)
                    thing.FinishCooldown();
            }
        }

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😙", (int)PlayerIconPriority.EatReaction, 1.0f);

        base.Use(user);
    }
}
