using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class EmptyPotion : Thing
{
    public override string AbilityName => "Fill Potion";

    public EmptyPotion()
    {
        DisplayIcon = "🧉";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;
        DisplayName = "Empty Potion";
        Description = "Can be filled with liquids";
        Tooltip = "An empty potion";
        Flammability = 13;

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "😋", $"Consume potion to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f), isAbility: true);
            AddTrait("Fragile", "🧉", $"Potion breaks when it hits something", offset: new Vector2(0f, -1f), tattooIcon: "💥", tattooScale: 0.65f, tattooOffset: new Vector2(7f, 7f));
        }
    }

    public override void Use(Thing user)
    {
        base.Use(user);
        
    }
}
