using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollBlink : Thing
{
    [Net] public int Radius { get; set; }

    public int ReqInt { get; private set; }

    public override string ChatDisplayIcons => "📜✨";
    public override string AbilityName => "Read Scroll";

    public ScrollBlink()
	{
		DisplayIcon = "📜";
        DisplayName = "Scroll of Blink";
        Description = "Teleport to a target location nearby.";
        Tooltip = "A scroll of Blink.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
        Flags = ThingFlags.Selectable | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        SetTattoo("✨", scale: 0.5f, offset: new Vector2(1f, 0), offsetWielded: new Vector2(0f, 0f), offsetInfo: new Vector2(8f, 5f), offsetCharWielded: new Vector2(2f, 0f), offsetInfoWielded: new Vector2(3f, 2f));

        if (Game.IsServer)
        {
            ReqInt = 2;
            AddTrait(AbilityName, "🔥", "Sacrifice scroll to cast the inscribed spell.", offset: new Vector2(0f, -2f), tattooIcon: "📜", tattooScale: 0.45f, tattooOffset: new Vector2(0f, 4f));
            AddTrait("", "🧠", $"{ReqInt}🧠 required to read.", offset: new Vector2(0f, -1f), labelText: $"≥{ReqInt}", labelFontSize: 15, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool TryStartUsing(Thing user)
    {
        var intelligence = user.GetStatClamped(StatType.Intelligence);
        if (intelligence < ReqInt)
        {
            if (user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {ReqInt}🧠 to use {ChatDisplayIcons} but you only have {intelligence}🧠");

            return false;
        }

        return true;
    }

    public override void Use(Thing user, IntVector targetGridPos)
    {
        base.Use(user, targetGridPos);

        var things = ContainingGridManager.GetThingsAt(targetGridPos).WithAll(ThingFlags.Solid).ToList();
        if (things.Count > 0)
            return;

        RoguemojiGame.Instance.AddFloater("✨", user.GridPos, 0.8f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), "", requireSight: true, EasingType.SineOut, fadeInTime: 0.2f);
        RoguemojiGame.Instance.AddFloater("✨", targetGridPos, 0.5f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), "", requireSight: true, EasingType.SineOut, fadeInTime: 0.1f);

        user.SetGridPos(targetGridPos);

        Destroy();
    }

    public override void OnWieldedBy(Thing thing)
    {
        base.OnWieldedBy(thing);

        Radius = Math.Max(thing.GetStatClamped(StatType.Intelligence), 1);
    }

    public override HashSet<IntVector> GetAimingTargetCellsClient() 
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        return ScrollBlink.BlinkGetAimingCells(Radius, ThingWieldingThis);
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        return ScrollBlink.BlinkIsPotentialAimingCell(gridPos, Radius, ThingWieldingThis);
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
                if (thingWieldingThis.ContainingGridManager.GetThingsAtClient(gridPos).WithAll(ThingFlags.Solid).ToList().Count > 0)
                    continue;

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
