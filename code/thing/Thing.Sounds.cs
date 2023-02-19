using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Thing : Entity
{
    [Net] public ThingSoundProfileType SoundProfileType { get; set; }

    public void PlaySfx(SoundActionType actionType)
    {
        var gridPos = ThingWieldingThis?.GridPos ?? GridPos;
        var levelId = ThingWieldingThis?.CurrentLevelId ?? CurrentLevelId;
        var level = RoguemojiGame.Instance.GetLevel(levelId);

        GetSound(actionType, level.SurfaceType, out string sfxName, out int loudness);
        if (!string.IsNullOrEmpty(sfxName))
            level.GridManager.PlaySfx(sfxName, gridPos, loudness);
    }

    public virtual void GetSound(SoundActionType actionType, SurfaceType surfaceType, out string sfxName, out int loudness)
    {
        sfxName = "";
        loudness = 0;

        switch (SoundProfileType)
        {
            case ThingSoundProfileType.Default:
                switch (actionType)
                {
                    case SoundActionType.Move:
                        switch (surfaceType)
                        {
                            case SurfaceType.Grass: sfxName = "footstep_grass"; break;
                        }
                        break;
                    case SoundActionType.Drop:
                        switch (surfaceType)
                        {
                            case SurfaceType.Grass: sfxName = "drop_grass"; break;
                        }
                        break;
                    case SoundActionType.GetHit:
                        sfxName = "impact";
                        loudness = 1;
                        break;
                    case SoundActionType.Throw:
                        sfxName = "throw";
                        loudness = 1;
                        break;
                    case SoundActionType.Wield:
                        sfxName = "wield";
                        loudness = 1;
                        break;
                }
                break;
        }
    }
}
