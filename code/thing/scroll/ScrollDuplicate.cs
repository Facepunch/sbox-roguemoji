using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollDuplicate : Scroll
{
    public ScrollDuplicate()
    {
        ScrollType = ScrollType.Duplicate;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Create a copy of something";
        Tooltip = "A scroll of Duplicate";

        SetTattoo(Globals.Icon(IconType.Duplicate));

        if (Game.IsServer)
        {
            //AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("🤗", (int)PlayerIconPriority.UseScroll, 1.0f);

        var targetThing = user.ContainingGridManager.GetThingsAt(targetGridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        if (targetThing == null)
        {
            Destroy();
            return;
        }

        targetThing.AddFloater($"{Globals.Icon(IconType.Duplicate)}", 1f, new Vector2(0, 3f), new Vector2(0, -12f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f, scale: 0.7f, opacity: 0.7f);

        if (user.ContainingGridManager.GetRandomEmptyGridPosWithinRange(targetThing.GridPos, out var emptyGridPos, 1, allowNonSolid: true))
        {
            var clone = user.ContainingGridManager.SpawnThing(TypeLibrary.GetType(targetThing.GetType()), emptyGridPos);
            clone.VfxFly(targetThing.GridPos, 0.25f, 30f, progressEasingType: EasingType.SineInOut);
        }

        Destroy();
    }
}
