using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Interfacer;

public enum Direction { Left, Right, Down, Up }

[Flags]
public enum ThingFlags
{
	None = 0,
	Solid = 1,
	Selectable = 2,
}

public class GridManager
{
	public int GridWidth { get; private set; }
	public int GridHeight { get; private set; }

	public GridPanelType GridPanelType { get; private set; }

	public Dictionary<IntVector, List<Thing>> GridThings = new Dictionary<IntVector, List<Thing>>();

	public GridManager(int width, int height, GridPanelType gridPanelType)
	{
		GridWidth = width;
		GridHeight = height;
		GridPanelType = gridPanelType;
	}

	public int GetIndex( int x, int y ) { return y * GridWidth + x; }
	public IntVector GetGridPos( int index ) { return new IntVector( index % GridWidth, ((float)index / (float)GridWidth).FloorToInt() ); }
	public Vector2 GetScreenPos(IntVector gridPos) { return new Vector2(gridPos.x, gridPos.y) * 40f; }

	public static int GetIndex( int x, int y, int width) { return y * width + x; }
	public static IntVector GetGridPos( int index, int width) { return new IntVector( index % width, ((float)index / (float)width).FloorToInt() ); }

	public bool IsGridPosInBounds( int x, int y )
	{
		return
			x >= 0 &&
			x < GridWidth &&
			y >= 0 &&
			y < GridHeight;
	}

	public bool IsGridPosInBounds( IntVector gridPos )
	{
		return
			gridPos.x >= 0 &&
			gridPos.x < GridWidth &&
			gridPos.y >= 0 &&
			gridPos.y < GridHeight;
	}

    public void SetGridPos(Thing thing, IntVector gridPos)
    {
        if (!IsGridPosInBounds(gridPos))
            return;

        IntVector currGridPos = thing.GridPos;
        DeregisterGridPos(thing, currGridPos);
        RegisterGridPos(thing, gridPos);
    }

    public void RegisterGridPos(Thing thing, IntVector gridPos)
    {
        if (GridThings.ContainsKey(gridPos))
            GridThings[gridPos].Add(thing);
        else
            GridThings[gridPos] = new List<Thing> { thing };
    }

    public void DeregisterGridPos(Thing thing, IntVector gridPos)
    {
        if (GridThings.ContainsKey(gridPos))
        {
            GridThings[gridPos].Remove(thing);
        }
    }

    public static IntVector GetIntVectorForDirection(Direction direction)
	{
		switch(direction)
		{
			case Direction.Left:  return new IntVector( -1, 0 );
			case Direction.Right: return new IntVector( 1, 0 );
			case Direction.Down: return new IntVector( 0, 1 );
			case Direction.Up: default: return new IntVector( 0, -1 );
		}
	}

	public static Vector2 GetVectorForDirection(Direction direction)
	{
		switch (direction)
		{
			case Direction.Left: return new Vector2(-1f, 0f);
			case Direction.Right: return new Vector2(1f, 0f);
			case Direction.Down: return new Vector2(0f, 1f);
			case Direction.Up: default: return new Vector2(0f, -1f);
		}
	}

	public static string GetDirectionText(Direction direction)
	{
		string output = "";

		switch ( direction )
		{
			case Direction.Left:
				output = "left";
				break;
			case Direction.Right:
				output = "right";
				break;
			case Direction.Down:
				output = "down";
				break;
			case Direction.Up:
			default:
				output = "up";
				break;
		}

		return output;
	}

	public bool DoesThingExistAt(IntVector gridPos, ThingFlags flags = ThingFlags.None)
	{
		if ( !GridThings.ContainsKey( gridPos ) )
			return false;

		var things = GridThings[gridPos];
		if ( things == null || things.Count == 0 )
			return false;

		return things.Where(x => (x.Flags & flags) == 0).Count() > 0;
	}

	public Thing GetThingAt(IntVector gridPos, ThingFlags flags = ThingFlags.None)
	{
		if ( !GridThings.ContainsKey( gridPos ) )
			return null;

		var things = GridThings[gridPos];
		if ( things == null || things.Count == 0 )
			return null;

		Thing highestThing = null;
		float highestDepth = -float.MaxValue;
		
		foreach ( var thing in things )
        {
			if ((thing.Flags & flags) == 0)
				continue;

			if(thing.IconDepth > highestDepth)
            {
				highestThing = thing;
				highestDepth = thing.IconDepth;
            }
        }

		return highestThing;
	}

	public List<Thing> GetThingsAt( IntVector gridPos )
	{
		if ( !GridThings.ContainsKey( gridPos ) )
			return null;

		return GridThings[gridPos];
	}
}
