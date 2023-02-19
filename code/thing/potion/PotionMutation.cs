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
        if (!thing.HasFlag(ThingFlags.CanGainMutations))
            return;

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

            thing.AddFloater(Globals.Icon(IconType.Mutation), 1.2f, new Vector2(0, -3f), new Vector2(0, -12f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.15f);
        }
    }

    List<TypeDescription> GetPossibleMutations()
    {
        return new List<TypeDescription>() { 
            TypeLibrary.GetType(typeof(MTeleportitis)),
            TypeLibrary.GetType(typeof(MSeeInvisible)),
            TypeLibrary.GetType(typeof(MPoisonSpeed)),
            TypeLibrary.GetType(typeof(MAllergicNuts)),
        };
    }
}
