using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionMutation : Potion
{
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
        var possibleMutations = GetPossibleMutations();

        for(int i = possibleMutations.Count - 1; i >= 0; i--)
        {
            var mutationType = possibleMutations[i];
            if(thing.HasComponent(mutationType))
                possibleMutations.RemoveAt(i);
        }

        if(possibleMutations.Count > 0)
        {
            var selectedType = possibleMutations[Game.Random.Int(0, possibleMutations.Count - 1)];
            thing.AddComponent(selectedType);
        }
    }

    List<TypeDescription> GetPossibleMutations()
    {
        return new List<TypeDescription>() { 
            TypeLibrary.GetType(typeof(MTeleportitis)),
            TypeLibrary.GetType(typeof(MSeeInvisible)),
        };
    }
}
