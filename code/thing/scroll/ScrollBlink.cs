using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollBlink : Scroll
{
    public override string ChatDisplayIcons => $"📜{Globals.Icon(IconType.Blink)}";
    public override string AbilityName => "Read Scroll";

    public ScrollBlink()
	{
        ScrollType = ScrollType.Blink;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = "Scroll of Blink";
        Description = "Teleport to a target place nearby";
        Tooltip = "A scroll of Blink";

        SetTattoo(Globals.Icon(IconType.Blink), scale: 0.5f, offset: new Vector2(1f, -2f), offsetWielded: new Vector2(0f, 0.3f), offsetInfo: new Vector2(8f, 5f), offsetCharWielded: new Vector2(0.5f, -2.5f), offsetInfoWielded: new Vector2(3f, 2f));

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "🔥", $"Sacrifice to cast the inscribed spell", offset: new Vector2(0f, -2f), tattooIcon: "📜", tattooScale: 0.45f, tattooOffset: new Vector2(0f, 4f));
            AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, IntVector targetGridPos)
    {
        base.Use(user, targetGridPos);

        var things = user.ContainingGridManager.GetThingsAt(targetGridPos).WithAll(ThingFlags.Solid).ToList();
        if (things.Count > 0)
        {
            if (user.ContainingGridManager.GetRandomEmptyAdjacentGridPos(targetGridPos, out var emptyGridPos, allowNonSolid: true))
                targetGridPos = emptyGridPos;
            else
                targetGridPos = user.GridPos;
        }

        RoguemojiGame.Instance.AddFloater("✨", user.GridPos, 0.8f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, EasingType.SineOut, fadeInTime: 0.2f);
        RoguemojiGame.Instance.AddFloater("✨", targetGridPos, 0.5f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, EasingType.SineOut, fadeInTime: 0.1f);

        user.SetGridPos(targetGridPos);

        if (user is RoguemojiPlayer player)
            player.RecenterCamera();

        Destroy();
    }

    public override HashSet<IntVector> GetAimingTargetCellsClient() 
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        int radius = Math.Clamp(ThingWieldingThis.GetStatClamped(StatType.Intelligence), 1, 10);
        return ScrollBlink.BlinkGetAimingCells(radius, ThingWieldingThis);
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        int radius = Math.Clamp(ThingWieldingThis.GetStatClamped(StatType.Intelligence), 1, 10);
        return ScrollBlink.BlinkIsPotentialAimingCell(gridPos, radius, ThingWieldingThis);
    }

    public static HashSet<IntVector> BlinkGetAimingCells(int radius, Thing thingWieldingThis)
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
                //if (thingWieldingThis.ContainingGridManager.GetThingsAtClient(gridPos).WithAll(ThingFlags.Solid).ToList().Count > 0)
                //    continue;

                aimingCells.Add(gridPos);
            }
        }

        return aimingCells;
    }

    public static bool BlinkIsPotentialAimingCell(IntVector gridPos, int radius, Thing thingWieldingThis)
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
