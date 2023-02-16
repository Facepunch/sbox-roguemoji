using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionWater : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Water);

    public PotionWater()
    {
        PotionType = PotionType.Water;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Fresh clean water";
        Tooltip = "A water potion";
        
        SetTattoo(Globals.Icon(IconType.Water));

        if (Game.IsServer)
        {
            AddTrait("", Globals.Icon(IconType.Water), $"Puts out fires", offset: new Vector2(0f, 0f));
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

        if (!user.ContainingGridManager.DoesGridPosContainThingType<PuddleWater>(user.GridPos))
        {
            user.ContainingGridManager.RemovePuddles(user.GridPos);
            user.ContainingGridManager.SpawnThing<PuddleWater>(user.GridPos);
        }

        base.Use(user);
    }

    public override void ApplyEffectToThing(Thing thing)
    {
        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😅", (int)PlayerIconPriority.WaterWet, 1.0f);
    }

    public override void ApplyEffectToGridPos(GridManager gridManager, IntVector gridPos)
    {
        if (!gridManager.DoesGridPosContainThingType<PuddleWater>(gridPos))
        {
            gridManager.RemovePuddles(gridPos);
            gridManager.SpawnThing<PuddleWater>(gridPos);
        }
    }
}
