using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollDisplace : Scroll
{
    public ScrollDisplace()
	{
        ScrollType = ScrollType.Displace;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Forces things to teleport";
        Tooltip = "A scroll of Displace";

        SetTattoo(Globals.Icon(IconType.Displace));

        if (Game.IsServer)
        {
            AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        var things = user.ContainingGridManager.GetThingsAt(targetGridPos).WithAll(ThingFlags.Selectable).ToList();
        foreach(var thing in things)
            ScrollTeleport.TeleportThing(thing, showStartFloater: false);

        user.ContainingGridManager.AddFloater(Globals.Icon(IconType.Teleport), targetGridPos, 0.8f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();
    }
}
