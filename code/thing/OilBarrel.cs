using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class OilBarrel : Thing
{
    public int RemainingOilAmount { get; set; }

	public OilBarrel()
	{
		DisplayIcon = "️🛢";
        DisplayName = "Oil Barrel";
        Description = "An open barrel of flammable oil";
        Tooltip = "A barrel of oil";
        IconDepth = (int)IconDepthLevel.Solid;
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        PathfindMovementCost = 20f;
        Flammability = 30;
        RemainingOilAmount = 4;

        //SetTattoo("⚫️", scale: 0.475f, offset: new Vector2(-0.858505f, 2f), offsetWielded: new Vector2(-1.5f, 6f), offsetInfo: new Vector2(-3.5f, 16f), offsetCharWielded: new Vector2(0f, 8f), offsetInfoWielded: new Vector2(-4.75f, 6.25f));
    }

    public override void OnBumpedIntoBy(Thing thing, Direction direction)
    {
        base.OnBumpedIntoBy(thing, direction);
        SquirtOil();        
    }

    public override void OnBumpedIntoThing(Thing thing, Direction direction)
    {
        base.OnBumpedIntoThing(thing, direction);
        SquirtOil();
    }

    void SquirtOil()
    {
        if (RemainingOilAmount > 0)
        {
            if (GetRandomNearbyPos(out var nearbyGridPos))
                SpawnOil(nearbyGridPos);
            else if (ContainingGridManager.GetRandomEmptyAdjacentGridPos(GridPos, out var nearbyGridPos2, allowNonSolid: true))
                SpawnOil(nearbyGridPos2);
        }
        else
        {
            var startOffset = new Vector2(8f, -10f);
            var endOffset = startOffset + new Vector2(0f, Game.Random.Float(-20f, -25f));
            var time = Game.Random.Float(0.4f, 0.55f);
            var scale = 0.6f;
            var opacity = 0.7f;

            AddFloater("☁️", time, startOffset, endOffset, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.025f, scale: scale, opacity: opacity, shakeAmount: 0f);
        }
    }

    void SpawnOil(IntVector gridPos)
    {
        ContainingGridManager.RemovePuddles(gridPos, fadeOut: true);

        var oil = ContainingGridManager.SpawnThing<PuddleOil>(gridPos);
        oil.VfxFly(GridPos, lifetime: 0.25f, heightY: 35f, progressEasingType: EasingType.Linear, heightEasingType: EasingType.SineInOut);
        oil.CanBeSeenByPlayerClient(GridPos);

        var tempIconDepth = oil.AddComponent<CTempIconDepth>();
        tempIconDepth.Lifetime = 0.35f;
        tempIconDepth.SetTempIconDepth((int)IconDepthLevel.Projectile);

        RemainingOilAmount--;
        if (RemainingOilAmount == 0)
        {
            DisplayName = "Empty Barrel";
            Description = "An empty barrel";
            Tooltip = "An empty barrel";
        }
    }

    bool GetRandomNearbyPos(out IntVector gridPos)
    {
        List<IntVector> gridPositions = new();
        gridPos = GridPos;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var currGridPos = GridPos + new IntVector(x, y);
                if (!ContainingGridManager.IsGridPosInBounds(currGridPos))
                    continue;

                if (ContainingGridManager.GetThingsAt(currGridPos).WithAll(ThingFlags.Puddle).WithNone(ThingFlags.Solid).Count() == 0)
                    gridPositions.Add(currGridPos);
            }
        }

        if (gridPositions.Count > 0)
        {
            gridPos = gridPositions[Game.Random.Int(0, gridPositions.Count - 1)];
            return true;
        }

        return false;
    }
}
