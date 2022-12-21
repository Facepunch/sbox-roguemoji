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

        var explosion = user.ContainingGridManager.SpawnThing<Explosion>(targetGridPos);
        explosion.VfxShake(0.15f, 6f);
        explosion.VfxScale(0.15f, 0.5f, 1f);
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
}
