using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CExitingLevel : ThingComponent
{
    public float Lifetime { get; set; }
    public LevelId TargetLevelId { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        Lifetime = 0.5f;

        if(thing.Brain is RoguemojiPlayer player)
            player.VfxFadeCamera(lifetime: 0.45f, shouldFadeOut: true);

        thing.VfxScale(0.45f, 1f, 0.5f);

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component))
            IconId = ((CIconPriority)component).AddIconPriority("😮", (int)PlayerIconPriority.ExitLevel);

        Thing.IsInTransit = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;

        if(TimeElapsed > Lifetime)
        {
            if (Thing.Brain is RoguemojiPlayer player)
                RoguemojiGame.Instance.ChangeThingLevel(Thing, TargetLevelId, shouldAnimateFall: true);

            Thing.AddComponent<CEnteringLevel>();

            Remove();
        }
    }

    public override void OnRemove()
    {
        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);
    }
}