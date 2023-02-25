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

        SetTattoo(Globals.Icon(IconType.Fire));

        if (Game.IsServer)
        {
            AddTrait("", Thing.GetStatIcon(StatType.Attack), $"Spell damage increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(0f, 0f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😤", (int)PlayerIconPriority.UseScroll, 1.0f);

        ShootFireball(user, targetGridPos);

        Destroy();
    }

    public static void ShootFireball(Thing user, IntVector targetGridPos)
    {
        var worldTarget = user.ContainingGridManager.GetWorldPosForGridPos(targetGridPos);
        var worldStart = user.ContainingGridManager.GetWorldPosForGridPos(user.GridPos);
        Vector2 dir = (worldTarget - worldStart).Normal;

        var fireball = user.ContainingGridManager.SpawnThing<ProjectileFireball>(user.GridPos);
        var degrees = (-dir).Degrees;
        fireball.SetTransformClient(degrees: degrees);
        fireball.ExplosionDamage = Math.Max(user.GetStatClamped(StatType.Intelligence), 3);

        var projectile = fireball.AddComponent<CProjectile>();
        projectile.MoveDelay = 0.115f;
        projectile.TotalDistance = 10;
        projectile.Thrower = user;
        projectile.UseDirectionVector(dir);
    }

    public override void GetSound(SoundActionType actionType, SurfaceType surfaceType, out string sfxName, out int loudness)
    {
        switch (actionType)
        {
            case SoundActionType.Use:
                sfxName = "fireball_cast";
                loudness = 3;
                return;
        }

        base.GetSound(actionType, surfaceType, out sfxName, out loudness);
    }
}
