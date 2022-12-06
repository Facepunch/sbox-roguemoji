using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Roguemoji;

public enum Direction { None, Left, Right, Down, Up }

public enum GridType { None, Arena, Inventory, Equipment }

public partial class GridManager : Entity
{
	[Net] public int GridWidth { get; private set; }
	[Net] public int GridHeight { get; private set; }

    [Net] public IList<Thing> Things { get; private set; }
	[Net] public GridType GridType { get; set; }

    public Dictionary<IntVector, List<Thing>> GridThings = new Dictionary<IntVector, List<Thing>>();

	public RoguemojiPlayer OwningPlayer { get; set; }

	public void Init(int width, int height)
	{
		GridWidth = width;
		GridHeight = height;

		Transmit = TransmitType.Always;

		Things = new List<Thing>();
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

			// todo: only update if a status needs updating
			if (thing.ShouldUpdate || thing.Statuses.Count > 0)
				thing.Update(dt);
		}
	}

    public T SpawnThing<T>(IntVector gridPos) where T : Thing
    {
        Host.AssertServer();

        var thing = TypeLibrary.GetDescription(typeof(T)).Create<T>();
        AddThing(thing);

        thing.SetGridPos(gridPos);

        return thing;
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

	void RefreshStackNums(IntVector gridPos)
	{
        var things = GridThings[gridPos];
		if (things == null || things.Count == 0)
			return;

		int stackNum = 0;
		foreach(var thing in things)
			thing.StackNum = stackNum++;
    }

	public int GetIndex( IntVector gridPos ) { return gridPos.y * GridWidth + gridPos.x; }
	public IntVector GetGridPos( int index ) { return new IntVector( index % GridWidth, ((float)index / (float)GridWidth).FloorToInt() ); }
	public Vector2 GetScreenPos(IntVector gridPos) { return new Vector2(gridPos.x, gridPos.y) * 40f; }

	public static int GetIndex( int x, int y, int width) { return y * width + x; }
	public static IntVector GetGridPos( int index, int width) { return new IntVector( index % width, ((float)index / (float)width).FloorToInt() ); }

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

        RefreshStackNums(gridPos);
    }

    public void DeregisterGridPos(Thing thing, IntVector gridPos)
    {
        if (GridThings.ContainsKey(gridPos))
        {
            GridThings[gridPos].Remove(thing);
			RefreshStackNums(gridPos);
        }
    }

    public static IntVector GetIntVectorForDirection(Direction direction)
	{
		switch(direction)
		{
			case Direction.Left:  return new IntVector( -1, 0 );
			case Direction.Right: return new IntVector( 1, 0 );
			case Direction.Down: return new IntVector( 0, 1 );
			case Direction.Up: return new IntVector( 0, -1 );
		}

		return IntVector.Zero;
	}

	public static Vector2 GetVectorForDirection(Direction direction)
	{
		switch (direction)
		{
			case Direction.Left: return new Vector2(-1f, 0f);
			case Direction.Right: return new Vector2(1f, 0f);
			case Direction.Down: return new Vector2(0f, 1f);
			case Direction.Up: return new Vector2(0f, -1f);
		}

		return Vector2.Zero;
	}

    public static Direction GetDirectionForIntVector(IntVector vec)
    {
		if (vec.x == -1 && vec.y == 0)
			return Direction.Left;
		else if (vec.x == 1 && vec.y == 0)
            return Direction.Right;
        else if (vec.x == 0 && vec.y == -1)
            return Direction.Up;
        else if (vec.x == 0 && vec.y == 1)
            return Direction.Down;

		return Direction.None;
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
				output = "up";
				break;
		}

		return output;
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

	public float GetPathfindMovementCost(IntVector gridPos)
	{
		float movementCost = 0f;
		var things = GetThingsAt(gridPos).WithAll(ThingFlags.Solid);
		foreach(var thing in things)
		{
			movementCost += thing.PathfindMovementCost;
		}

		return movementCost;
	}

	public bool GetFirstEmptyGridPos(out IntVector gridPos)
	{
		for(int index = 0; index < GridWidth * GridHeight; index++) 
		{
			var currGridPos = GetGridPos(index);
            var things = GetThingsAt(currGridPos);

            if (things.Count() == 0)
			{
				gridPos = currGridPos;
				return true;
			}
		}

		gridPos = IntVector.Zero;
		return false;
	}

	public bool GetRandomEmptyGridPos(out IntVector gridPos)
	{
		int NUM_TRIES = 100;
		for(int i = 0; i < NUM_TRIES; i++)
		{
			var currGridPos = new IntVector(Rand.Int(0, GridWidth - 1), Rand.Int(0, GridHeight - 1));
			var things = GetThingsAt(currGridPos);
            if (things.Count() == 0)
            {
                gridPos = currGridPos;
                return true;
            }
        }

        gridPos = IntVector.Zero;
        return false;
    }

	public void Restart()
	{
        foreach (var thing in Things)
        {
            if (thing is not RoguemojiPlayer)
                thing.Delete();
        }
		Things.Clear();

		GridThings.Clear();
    }

	public void PrintGridThings()
	{
		Log.Info("----------- " + GridType);

        //public Dictionary<IntVector, List<Thing>> GridThings = new Dictionary<IntVector, List<Thing>>();

		foreach(var pair in GridThings)
		{
			Log.Info(pair.Key + ": " + (pair.Value?.Count ?? 0));
		}
	}
}
