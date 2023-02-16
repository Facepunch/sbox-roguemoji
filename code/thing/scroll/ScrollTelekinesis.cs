using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollTelekinesis : Scroll
{
    public ScrollTelekinesis()
    {
        ScrollType = ScrollType.Telekinesis;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Pull target with your mind";
        Tooltip = "A scroll of Telekinesis";

        SetTattoo(Globals.Icon(IconType.Telekinesis));

        if (Game.IsServer)
        {
            AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        var targetThing = user.ContainingGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        if (targetThing == null || targetThing == user)
        {
            user.ContainingGridManager.AddFloater($"{Globals.Icon(IconType.Telekinesis)}", targetGridPos, 0.5f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);
            Destroy();
            return;
        }

        if (targetThing.HasFlag(ThingFlags.Solid))
        {
            Direction pullDirection = GridManager.GetDirectionForIntVector(GridManager.GetIntVectorForSlope(targetGridPos, user.GridPos));

            var player = (targetThing.Brain != null && targetThing.Brain is RoguemojiPlayer) ? (RoguemojiPlayer)targetThing.Brain : null;
            bool success = player != null
                ? player.TryMove(pullDirection, dontRequireAction: true)
                : targetThing.TryMove(pullDirection, out bool switchedLevel, dontRequireAction: true);

            if (targetThing.GetComponent<CActing>(out var c))
                ((CActing)c).PerformedAction();

            targetThing.AddFloater($"{Globals.Icon(IconType.Telekinesis)}", 0.5f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);
        }
        else
        {
            var startingPos = targetThing.GridPos;
            targetThing.SetGridPos(user.GridPos);
            targetThing.VfxFly(startingPos, lifetime: 0.2f, heightY: 30f, progressEasingType: EasingType.Linear, heightEasingType: EasingType.SineInOut);
            user.ContainingGridManager.AddFloater($"{Globals.Icon(IconType.Telekinesis)}", targetGridPos, 0.5f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);
        }

        user.AddFloater($"{Globals.Icon(IconType.Telekinesis)}", 0.5f, new Vector2(0, -3f), new Vector2(0, -8f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);

        Destroy();
    }

    public override HashSet<IntVector> GetAimingTargetCellsClient()
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        int radius = Math.Clamp(ThingWieldingThis.GetStatClamped(StatType.Intelligence), 3, 12);
        return TelekinesisGetAimingCells(radius, ThingWieldingThis);
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        int radius = Math.Clamp(ThingWieldingThis.GetStatClamped(StatType.Intelligence), 3, 12);
        return TelekinesisIsPotentialAimingCell(gridPos, radius, ThingWieldingThis);
    }

    public static HashSet<IntVector> TelekinesisGetAimingCells(int radius, Thing thingWieldingThis)
    {
        HashSet<IntVector> aimingCells = new HashSet<IntVector>();

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int distance = Utils.GetDistance(x, y);
                if (distance > radius)
                    continue;

                var gridPos = thingWieldingThis.GridPos + new IntVector(x, y);
                aimingCells.Add(gridPos);
            }
        }

        return aimingCells;
    }

    public static bool TelekinesisIsPotentialAimingCell(IntVector gridPos, int radius, Thing thingWieldingThis)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int distance = Utils.GetDistance(x, y);
                if (distance > radius)
                    continue;

                var currGridPos = thingWieldingThis.GridPos + new IntVector(x, y);
                if (gridPos.Equals(currGridPos))
                    return true;
            }
        }

        return false;
    }
}
