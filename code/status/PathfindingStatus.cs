using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Interfacer;

public class PathfindingStatus : ThingStatus
{
    public List<IntVector> GridPath { get; private set; } = new List<IntVector>();
    
    public TimeSince TimeSincePathfind { get; private set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        TimeSincePathfind = 0f;
    }

    public List<IntVector> GetPathTo(IntVector a, IntVector b)
    {
        TimeSincePathfind = 0f;

        GridManager grid = Thing.ContainingGridManager;

        GridPath.Clear();

        if((a - b).ManhattanLength <= 1)
        {
            GridPath.Add(b);
            return GridPath;
        }

        List<IntVector> tempPath = new List<IntVector>();
        if(Utils.AStar<IntVector>(a, b, tempPath, GetEdges, GetHScoreFromGridPosToGridPos))
        {
            GridPath.AddRange(tempPath);

            // remove start pos
            GridPath.RemoveAt(0);
        }

        return GridPath;
    }

    private readonly List<IntVector> _walkable = new List<IntVector>();

    public List<IntVector> GetWalkableAdjacentGridPositions(IntVector start)
    {
        _walkable.Clear();
        GridManager grid = Thing.ContainingGridManager;

        IntVector left = start + new IntVector(-1, 0);
        if (grid.IsGridPosInBounds(left))
            _walkable.Add(left);

        IntVector right = start + new IntVector(1, 0);
        if (grid.IsGridPosInBounds(right))
            _walkable.Add(right);

        IntVector down = start + new IntVector(0, -1);
        if (grid.IsGridPosInBounds(down))
            _walkable.Add(down);

        IntVector up = start + new IntVector(0, 1);
        if (grid.IsGridPosInBounds(up))
            _walkable.Add(up);

        return _walkable;
    }


    static float GetHScoreFromGridPosToGridPos(IntVector a, IntVector b)
    {
        return (b - a).ManhattanLength;
    }

    IEnumerable<AStarEdge<IntVector>> GetEdges(IntVector start)
    {
        var walkable = GetWalkableAdjacentGridPositions(start);
        return walkable.Select(gridPos => Utils.Edge(gridPos, GetCostToMoveFromGridPosToAdjacentGridPos(start, gridPos)));
    }

    float GetCostToMoveFromGridPosToAdjacentGridPos(IntVector a, IntVector b)
    {
        GridManager grid = Thing.ContainingGridManager;
        float cost = 1f + grid.GetPathfindMovementCost(b);

        return cost;
    }
}