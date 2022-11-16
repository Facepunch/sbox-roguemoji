using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Interfacer;

public enum Direction { Left, Right, Down, Up }

public class GridManager
{
	public int GridWidth { get; private set; }
	public int GridHeight { get; private set; }

	public GridPanelType GridPanelType { get; private set; }

	public Dictionary<IntVector, List<Thing>> GridThings = new Dictionary<IntVector, List<Thing>>();
	public List<IntVector> GridCellsToRefresh = new List<IntVector>();

	public GridManager(int width, int height, GridPanelType gridPanelType)
	{
		GridWidth = width;
		GridHeight = height;
		GridPanelType = gridPanelType;
	}

	public void Update()
	{
		RefreshCells();
	}

	public int GetIndex( int x, int y ) { return y * GridWidth + x; }
	public IntVector GetGridPos( int index ) { return new IntVector( index % GridWidth, ((float)index / (float)GridWidth).FloorToInt() ); }
	public Vector2 GetScreenPos(IntVector gridPos) { return new Vector2(gridPos.x * 40f, gridPos.y * 40f); }

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
		if ( !IsGridPosInBounds( gridPos ) )
			return;

		IntVector currGridPos = thing.GridPos;
		DeregisterGridPos(thing, currGridPos);
		RegisterGridPos(thing, gridPos );
	}

	public void RegisterGridPos( Thing thing, IntVector gridPos )
	{
		if ( GridThings.ContainsKey( gridPos ) )
			GridThings[gridPos].Add( thing );
		else
			GridThings[gridPos] = new List<Thing> { thing };

		RefreshGridPos( gridPos );
	}

	public void DeregisterGridPos(Thing thing, IntVector gridPos)
	{
		if(GridThings.ContainsKey(gridPos))
		{
			GridThings[gridPos].Remove( thing );
			RefreshGridPos( gridPos );
		}
	}

	public void RefreshGridPos(IntVector gridPos)
	{
		//Log.Info("RefreshGridPos: " + gridPos);
		if ( !GridCellsToRefresh.Contains( gridPos ) )
			GridCellsToRefresh.Add( gridPos );
	}

	public void RefreshCells()
	{
		foreach(IntVector gridCell in GridCellsToRefresh)
			RefreshCell( gridCell );

		GridCellsToRefresh.Clear();
	}

	void RefreshCell(IntVector gridPos)
	{
		float currPriority = -1f;
		Thing currThing = null;

		if(GridThings.ContainsKey(gridPos))
		{
			foreach(var thing in GridThings[gridPos])
			{
				if(thing.IconPriority > currPriority)
				{
					currThing = thing;
					currPriority = thing.IconPriority;
				}
			}
		}

		string iconString = currThing?.DisplayIcon ?? "";
		int playerNum = currThing?.PlayerNum ?? 0;
		string tooltip = currThing?.Tooltip ?? "";
		Vector2 offset = currThing?.Offset ?? Vector2.Zero;
		float rotationDegrees = currThing?.RotationDegrees ?? 0f;
		float size = currThing?.IconScale ?? 1f;

		InterfacerGame.Instance.WriteCell(GridPanelType, gridPos, iconString, playerNum, tooltip, offset, rotationDegrees, size);
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

	public bool DoesThingExistAt(IntVector gridPos)
	{
		if ( !GridThings.ContainsKey( gridPos ) )
			return false;

		var things = GridThings[gridPos];
		if ( things == null || things.Count == 0 )
			return false;

		return true;
	}

	public Thing GetThingAt(IntVector gridPos)
	{
		if ( !GridThings.ContainsKey( gridPos ) )
			return null;

		var things = GridThings[gridPos];
		if ( things == null || things.Count == 0 )
			return null;

		return things[0];
	}

	public List<Thing> GetThingsAt( IntVector gridPos )
	{
		if ( !GridThings.ContainsKey( gridPos ) )
			return null;

		return GridThings[gridPos];
	}
}
