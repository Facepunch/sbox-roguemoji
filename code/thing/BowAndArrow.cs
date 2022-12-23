using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class BowAndArrow : Thing
{
	public BowAndArrow()
	{
		DisplayIcon = "🏹";
        DisplayName = "Bow and Arrow";
        Description = "Shoots arrows.";
        Tooltip = "A bow and arrow.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable | ThingFlags.UseRequiresAiming;
    }

    public override void Use(Thing user, Direction direction)
    {
        base.Use(user, direction);

        var arrow = user.ContainingGridManager.SpawnThing<ProjectileArrow>(user.GridPos);
        arrow.SetTransformClient(degrees: GridManager.GetDegreesForDirection(direction));

        var projectile = arrow.AddComponent<Projectile>();
        projectile.Direction = direction;
        projectile.MoveDelay = 0.1f;
        projectile.RemainingDistance = 8;
    }
}
