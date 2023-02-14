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
        GetSound(actionType, RoguemojiGame.Instance.GetLevel(CurrentLevelId).SurfaceType, out string sfxName, out int loudness);
        if (!string.IsNullOrEmpty(sfxName))
            RoguemojiGame.Instance.PlaySfxArena(sfxName, GridPos, CurrentLevelId, loudness);
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
                    case SoundActionType.GetHit:
                        sfxName = "impact";
                        loudness = 1;
                        break;
                    case SoundActionType.Drop:
                        switch (surfaceType)
                        {
                            case SurfaceType.Grass: sfxName = "drop_grass"; break;
                        }
                        break;
                    case SoundActionType.Select:
                        sfxName = "click";
                        loudness = 0;
                        break;
                }
                break;
        }
    }
}
