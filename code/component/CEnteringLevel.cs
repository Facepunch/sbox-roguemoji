using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CEnteringLevel : ThingComponent
{
    public float Lifetime { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        float fallTime = (thing.GridPos.y + 3) * Utils.Map(thing.GridPos.y, 0, thing.ContainingGridManager.GridHeight - 1, 0.25f, 0.075f, EasingType.ExpoOut);

        Lifetime = fallTime;

        if(thing.Brain is RoguemojiPlayer player)
            player.VfxFadeCamera(lifetime: fallTime, shouldFadeOut: false);

        thing.VfxFly(startingGridPos: new IntVector(thing.GridPos.x, -3), lifetime: fallTime, progressEasingType: EasingType.SineIn);
        thing.VfxScale(0f, 1f, 1f);

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component))
            IconId = ((CIconPriority)component).AddIconPriority("🙃", (int)PlayerIconPriority.EnterLevel);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;

        if(TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.IsInTransit = false;

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);
    }
}