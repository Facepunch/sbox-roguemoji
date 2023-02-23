using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollFireball : Scroll
{
    public ScrollFireball()
    {
        ScrollType = ScrollType.Fireball;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Launch an explosive ball of fire";
        Tooltip = "A scroll of Fireball";

        SetTattoo(Globals.Icon(IconType.Burning));

        if (Game.IsServer)
        {
            AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        var worldTarget = user.ContainingGridManager.GetWorldPosForGridPos(targetGridPos);
        var worldStart = user.ContainingGridManager.GetWorldPosForGridPos(user.GridPos);
        Vector2 dir = (worldTarget - worldStart).Normal;

        var fireball = user.ContainingGridManager.SpawnThing<ProjectileFireball>(user.GridPos);
        var degrees = (-dir).Degrees;
        fireball.SetTransformClient(degrees: degrees);

        var projectile = fireball.AddComponent<CProjectile>();
        projectile.MoveDelay = 0.115f;
        projectile.TotalDistance = 10;
        projectile.Thrower = user;
        projectile.UseDirectionVector(dir);

        Destroy();
    }
}
