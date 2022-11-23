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
	VisualEffect = 4,
	InInventory = 8,
}

public partial class GridManager : Entity
{
	[Net] public int GridWidth { get; private set; }
	[Net] public int GridHeight { get; private set; }

	[Net] public IList<Thing> Things { get; private set; }

	public Dictionary<IntVector, List<Thing>> GridThings = new Dictionary<IntVector, List<Thing>>();

	public void Init(int width, int height)
	{
		GridWidth = width;
		GridHeight = height;

		Transmit = TransmitType.Always;

		Things = new List<Thing>();
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		//Log.Info("GridManager:ClientTick");

	}

	[Event.Tick.Server]
	public void ServerTick()
	{
		//Log.Info("GridManager:ServerTick");
	}

	public void Update(float dt)
	{
		UpdateThings(Things, dt);
	}

	void UpdateThings(IList<Thing> things, float dt)
	{
		for (int i = things.Count - 1; i >= 0; i--)
		{
			var thing = things[i];

			if (!thing.DoneFirstUpdate)
				thing.FirstUpdate();

			if (thing.ShouldUpdate || thing.Statuses.Count > 0)
				thing.Update(dt);
		}
	}

	public void AddThing(Thing thing)
	{
		Things.Add(thing);
		thing.ContainingGridManager = this;
    }

	public void RemoveThing(Thing thing)
	{
        DeregisterGridPos(thing, thing.GridPos);

		Things.Remove(thing);
		thing.ContainingGridManager = null;
	}

	public int GetIndex( IntVector gridPos ) { return gridPos.y * GridWidth + gridPos.x; }
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
        Host.AssertServer();
        
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

    public IEnumerable<Thing> GetThingsAt(IntVector gridPos)
    {
        Host.AssertServer();
        return GridThings.TryGetValue(gridPos, out var list) ? list : Enumerable.Empty<Thing>();
    }

    public IEnumerable<Thing> GetThingsAtClient(IntVector gridPos)
    {
        Host.AssertClient();
		return Things.Where(x => x.GridPos.Equals(gridPos));
    }

	public bool GetFirstEmptyGridPos(out IntVector gridPos)
	{
		for(int index = 0; index < GridWidth * GridHeight; index++)
		{
			var currGridPos = GetGridPos(index);
            if (!DoesThingExistAt(currGridPos))
			{
				gridPos = currGridPos;
				return true;
			}
		}

		gridPos = IntVector.Zero;
		return false;
	}
}
