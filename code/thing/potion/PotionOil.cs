using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionOil : Potion
{
    public override string SplashIcon => "⚫️";

    public PotionOil()
    {
        PotionType = PotionType.Oil;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Flammable oil";
        Tooltip = "An oil potion";
        
        SetTattoo(Globals.Icon(IconType.Oil));

        if (Game.IsServer)
        {
            AddTrait("", Globals.Icon(IconType.Oil), $"Full of flammable oil", offset: new Vector2(0f, 0f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        return true;
    }

    public override void Use(Thing user)
    {
        ApplyEffectToThing(user);
        ApplyEffectToGridPos(user.ContainingGridManager, user.GridPos);
        Destroy();

        base.Use(user);
    }

    public override void ApplyEffectToThing(Thing thing)
    {
        PuddleWater.DouseFire(thing);

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😔", (int)PlayerIconPriority.MudSad, 1.0f);
    }

    public override void ApplyEffectToGridPos(GridManager gridManager, IntVector gridPos)
    {
        if (!gridManager.DoesGridPosContainThingType<PuddleOil>(gridPos))
        {
            gridManager.RemovePuddles(gridPos, fadeOut: true);
            gridManager.SpawnThing<PuddleOil>(gridPos);
        }
    }
}
