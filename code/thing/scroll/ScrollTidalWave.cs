using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollTidalWave : Scroll
{
    public ScrollTidalWave()
    {
        ScrollType = ScrollType.TidalWave;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Create a wave of water that pushes things";
        Tooltip = "A scroll of Tidal Wave";

        SetTattoo(Globals.Icon(IconType.Wave));

        if (Game.IsServer)
        {
            AddTrait("", Thing.GetStatIcon(StatType.Attack), $"Spell damage increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(0f, 0f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        CreateTidalWaves(user, targetGridPos);

        Destroy();
    }

    public static void CreateTidalWaves(Thing user, IntVector targetGridPos)
    {
        var worldTarget = user.ContainingGridManager.GetWorldPosForGridPos(targetGridPos);
        var worldStart = user.ContainingGridManager.GetWorldPosForGridPos(user.GridPos);
        Vector2 dir = (worldTarget - worldStart).Normal;
        IntVector gridDir = GridManager.GetIntVectorForSlope(user.GridPos, targetGridPos);

        IntVector gridOffset = Math.Abs(dir.x) > Math.Abs(dir.y) ? new IntVector(0, 1) : new IntVector(1, 0);
        IntVector gridPos0 = user.GridPos;
        IntVector gridPos1 = user.GridPos + gridOffset;
        IntVector gridPos2 = user.GridPos - gridOffset;

        CreateSingleWave(user, gridPos0, gridDir, dir, shouldPushStartPos: false);
        CreateSingleWave(user, gridPos1, gridDir, dir, shouldPushStartPos: true);
        CreateSingleWave(user, gridPos2, gridDir, dir, shouldPushStartPos: true);
    }

    public static void CreateSingleWave(Thing user, IntVector gridPos, IntVector gridDir, Vector2 dir, bool shouldPushStartPos)
    {
        if (!user.ContainingGridManager.IsGridPosInBounds(gridPos))
            return;

        var wave = user.ContainingGridManager.SpawnThing<ProjectileTidalWave>(gridPos);
        wave.SlamDamage = 5;

        var projectile = wave.AddComponent<CProjectile>();
        projectile.ShouldHit = false;
        projectile.MoveDelay = 0.17f;
        projectile.TotalDistance = 6;
        projectile.Thrower = user;
        projectile.UseDirectionVector(dir);

        if(shouldPushStartPos)
            wave.PushStartPosThings(gridDir);
    }

    public override void GetSound(SoundActionType actionType, SurfaceType surfaceType, out string sfxName, out int loudness)
    {
        switch (actionType)
        {
            case SoundActionType.Use:
                sfxName = "tidal_wave";
                loudness = 3;
                return;
        }

        base.GetSound(actionType, surfaceType, out sfxName, out loudness);
    }
}
