using Sandbox;
using System;

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

    public static Direction GetRetreatDirection(Thing fearfulThing, Thing fearedThing, bool cardinalOnly = true)
    {
        var gridManager = fearfulThing.ContainingGridManager;

        var fearfulPos = fearfulThing.GridPos;
        var fearedPos = fearedThing.GridPos;
        var diff = fearfulPos - fearedPos;

        if(diff.Equals(IntVector.Zero))
        {
            if(gridManager.GetRandomEmptyAdjacentGridPos(fearfulPos, out var gridPos, allowNonSolid: false, cardinalOnly))
                return GridManager.GetDirectionForIntVector(gridPos - fearfulPos);

            return Direction.None;
        }

        if(Math.Abs(diff.x) > Math.Abs(diff.y))
        {
            IntVector newGridPosX = fearfulPos + new IntVector(Math.Sign(diff.x), 0);
            if(gridManager.IsGridPosInBounds(newGridPosX))
                return GridManager.GetDirectionForIntVector(newGridPosX - fearfulPos);
        }

        IntVector newGridPosY = fearfulPos + new IntVector(0, Math.Sign(diff.y));
        if (gridManager.IsGridPosInBounds(newGridPosY))
            return GridManager.GetDirectionForIntVector(newGridPosY - fearfulPos);

        return Direction.None;
    }
}