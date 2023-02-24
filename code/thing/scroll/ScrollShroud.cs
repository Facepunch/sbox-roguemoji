using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollShroud : Scroll
{
    public ScrollShroud()
    {
        ScrollType = ScrollType.Shroud;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Target becomes invisible";
        Tooltip = "A scroll of Shroud";

        SetTattoo(Globals.Icon(IconType.Shroud));

        if (Game.IsServer)
        {
            AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😉", (int)PlayerIconPriority.UseScroll, 1.0f);

        var targetThing = user.ContainingGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        if (targetThing == null)
        {
            Destroy();
            return;
        }

        targetThing.AddComponent<CInvisible>();
        targetThing.AddFloater($"{Globals.Icon(IconType.Shroud)}", 1f, new Vector2(0, 3f), new Vector2(0, -12f), height: 0f, text: "", requireSight: false, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f, opacity: 0.7f);

        Destroy();
    }
}
