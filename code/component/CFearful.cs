﻿using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CFearful : ThingComponent
{
    public float TimeElapsed { get; set; }
    public float Lifetime { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        TimeElapsed = 0f;

        RoguemojiGame.Instance.AddFloater("💧", Thing.GridPos, time: 0f, Thing.CurrentLevelId, new Vector2(10f, -10f), Vector2.Zero, text: "", requireSight: true, EasingType.Linear, fadeInTime: 0.025f, scale: 0.75f, parent: Thing);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;

        if(Lifetime > 0f && TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        RoguemojiGame.Instance.RemoveFloater("💧", Thing.CurrentLevelId, parent: Thing);
    }

    public override void OnThingDestroyed()
    {
        RoguemojiGame.Instance.RemoveFloater("💧", Thing.CurrentLevelId, parent: Thing);
    }

    public static IntVector GetTargetRetreatPoint(IntVector startingPoint, IntVector avoidPoint, GridManager gridManager, int maxDistToCorner)
    {
        IntVector diff = startingPoint - avoidPoint;
        IntVector targetPos = new IntVector(diff.x < 0 ? 0 : gridManager.GridWidth - 1, diff.y < 0 ? 0 : gridManager.GridHeight - 1);

        return targetPos;
    }
}