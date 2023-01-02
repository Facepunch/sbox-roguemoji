using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollBlink : Thing
{
    [Net] public int Radius { get; set; }

	public ScrollBlink()
	{
		DisplayIcon = "📜";
        DisplayName = "Scroll of Blink";
        Description = "Blink to a target location nearby.";
        Tooltip = "A scroll of Blink.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        SetTattoo("✨", scale: 0.5f, offset: new Vector2(1f, 0), offsetWielded: new Vector2(0f, 0f), offsetInfo: new Vector2(8f, 5f), offsetCharWielded: new Vector2(2f, 0f), offsetInfoWielded: new Vector2(3f, 2f));
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

        HashSet<IntVector> aimingCells = new HashSet<IntVector>();

        for(int x = -Radius; x <= Radius; x++)
        {
            for(int y = -Radius; y <= Radius; y++)
            {
                int distance = Utils.GetDistance(x, y);
                if (distance > Radius)
                    continue;

                var gridPos = ThingWieldingThis.GridPos + new IntVector(x, y);
                if (ThingWieldingThis.ContainingGridManager.GetThingsAtClient(gridPos).WithAll(ThingFlags.Solid).ToList().Count > 0)
                    continue;

                aimingCells.Add(gridPos);
            }
        }

        return aimingCells;
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        for (int x = -Radius; x <= Radius; x++)
        {
            for (int y = -Radius; y <= Radius; y++)
            {
                int distance = Utils.GetDistance(x, y);
                if (distance > Radius)
                    continue;

                var currGridPos = ThingWieldingThis.GridPos + new IntVector(x, y);
                if (gridPos.Equals(currGridPos))
                    return true;
            }
        }

        return false;
    }
}
