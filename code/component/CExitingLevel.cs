using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CExitingLevel : ThingComponent
{
    public float Lifetime { get; set; }
    public LevelId TargetLevelId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        Lifetime = 0.5f;

        if(thing is RoguemojiPlayer player)
            player.VfxFadeCamera(lifetime: 0.45f, shouldFadeOut: true);

        thing.VfxScale(0.45f, 1f, 0.5f);

        Thing.IsInTransit = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;

        if(TimeElapsed > Lifetime)
        {
            if (Thing is RoguemojiPlayer player)
                RoguemojiGame.Instance.ChangeThingLevel(player, TargetLevelId, shouldAnimateFall: true);

            Thing.AddComponent<CEnteringLevel>();

            Remove();
        }
    }

    public override void OnRemove()
    {
        
    }
}