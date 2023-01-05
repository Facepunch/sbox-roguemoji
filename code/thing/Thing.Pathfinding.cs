using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Thing : Entity
{
    private List<IntVector> _gridPath;
    private List<IntVector> _walkable;

    public List<IntVector> GetPathTo(IntVector a, IntVector b)
    {
        if (_gridPath == null)
            _gridPath = new List<IntVector>();
        else
            _gridPath.Clear();

        _gridPath.Clear();

        if ((a - b).ManhattanLength <= 1)
        {
            _gridPath.Add(b);
            return _gridPath;
        }

        List<IntVector> tempPath = new List<IntVector>();
        if (Utils.AStar<IntVector>(a, b, tempPath, GetEdges, GetHScoreFromGridPosToGridPos))
        {
            _gridPath.AddRange(tempPath);

            // remove start pos
            _gridPath.RemoveAt(0);
        }

        return _gridPath;
    }

    public List<IntVector> GetWalkableAdjacentGridPositions(IntVector start)
    {
        if (_walkable == null)
            _walkable = new List<IntVector>();
        else
            _walkable.Clear();

        IntVector left = start + new IntVector(-1, 0);
        if (ContainingGridManager.IsGridPosInBounds(left))
            _walkable.Add(left);

        IntVector right = start + new IntVector(1, 0);
        if (ContainingGridManager.IsGridPosInBounds(right))
            _walkable.Add(right);

        IntVector down = start + new IntVector(0, -1);
        if (ContainingGridManager.IsGridPosInBounds(down))
            _walkable.Add(down);

        IntVector up = start + new IntVector(0, 1);
        if (ContainingGridManager.IsGridPosInBounds(up))
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
        return 1f + ContainingGridManager.GetPathfindMovementCost(b);
    }
}
