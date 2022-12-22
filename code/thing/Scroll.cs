using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class Scroll : Thing
{
	public Scroll()
	{
		DisplayIcon = "📜";
        DisplayName = "Scroll";
        Description = "Blink to a target location nearby.";
        Tooltip = "A magical scroll.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;
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

    public override HashSet<IntVector> GetAimingTargetCellsClient() 
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        HashSet<IntVector> aimingCells = new HashSet<IntVector>();

        for(int x = -3; x <= 3; x++)
        {
            for(int y = -3; y <= 3; y++)
            {
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

        for (int x = -3; x <= 3; x++)
        {
            for (int y = -3; y <= 3; y++)
            {
                var currGridPos = ThingWieldingThis.GridPos + new IntVector(x, y);
                if (gridPos.Equals(currGridPos))
                    return true;
            }
        }

        return false;
    }
}
