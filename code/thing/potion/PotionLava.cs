using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionLava : Potion
{
    public override string SplashIcon => "🟠";

    public PotionLava()
    {
        PotionType = PotionType.Lava;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Burning hot lava";
        Tooltip = "A lava potion";
        
        SetTattoo(Globals.Icon(IconType.Lava));

        if (Game.IsServer)
        {
            AddTrait("", Globals.Icon(IconType.Lava), $"Burning lava", offset: new Vector2(0f, 0f));
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

    public override void ApplyEffectToGridPos(GridManager gridManager, IntVector gridPos)
    {
        if (!gridManager.DoesGridPosContainThingType<PuddleLava>(gridPos))
        {
            gridManager.RemovePuddles(gridPos, fadeOut: true);
            gridManager.SpawnThing<PuddleLava>(gridPos);
        }
    }
}
