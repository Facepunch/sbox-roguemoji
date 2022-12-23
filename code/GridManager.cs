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

	public HashSet<RoguemojiPlayer> ContainedPlayers = new HashSet<RoguemojiPlayer>();
	public HashSet<RoguemojiPlayer> VisionChangedPlayers = new HashSet<RoguemojiPlayer>(); // players who need an update to their field of view

	[Net] public LevelId LevelId { get; set; }

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

		if(GridType == GridType.Arena)
		{
			foreach (var player in VisionChangedPlayers)
				player.RefreshVisibility();

			VisionChangedPlayers.Clear();
		}
	}

	void UpdateThings(IList<Thing> things, float dt)
	{
		for (int i = things.Count - 1; i >= 0; i--)
		{
			var thing = things[i];

			if (!thing.DoneFirstUpdate)
				thing.FirstUpdate();

			// todo: only update if a component needs updating
			if (thing.ShouldUpdate || thing.ThingComponents.Count > 0)
				thing.Update(dt);
		}
	}

    public T SpawnThing<T>(IntVector gridPos) where T : Thing
    {
        Game.AssertServer();

        var thing = TypeLibrary.GetType(typeof(T)).Create<T>();
        AddThing(thing);

        thing.SetGridPos(gridPos);
		thing.CurrentLevelId = LevelId;

        return thing;
    }

    public void AddThing(Thing thing)
	{
		Things.Add(thing);
		thing.ContainingGridManager = this;

		var player = thing as RoguemojiPlayer;
		if (player != null)
            AddPlayer(player);
    }

	public void RemoveThing(Thing thing)
	{
        DeregisterGridPos(thing, thing.GridPos);

		Things.Remove(thing);
		thing.ContainingGridManager = null;

        var player = thing as RoguemojiPlayer;
        if (player != null)
            RemovePlayer(player);
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

		if (GridType == GridType.Arena)
		{
			if (thing.SightBlockAmount > 0)
				CheckVisionChange(thing);

			CheckAimingChange(gridPos);
        }
    }

    public void DeregisterGridPos(Thing thing, IntVector gridPos)
    {
        if (GridThings.ContainsKey(gridPos))
        {
            GridThings[gridPos].Remove(thing);
			RefreshStackNums(gridPos);

			if (GridType == GridType.Arena)
			{
				if (thing.SightBlockAmount > 0)
					CheckVisionChange(thing);

				CheckAimingChange(gridPos);
			}
        }
    }

	void CheckVisionChange(Thing thing)
	{
		foreach(var player in ContainedPlayers)
		{
			var sight = player.GetStatClamped(StatType.Sight);
            if (VisionChangedPlayers.Contains(player) || thing == player || thing.SightBlockAmount < sight)
				continue;

			var dist = GetDistance(player.GridPos, thing.GridPos);
			if (dist > sight)
				continue;

			VisionChangedPlayers.Add(player);
		}
	}

	void CheckAimingChange(IntVector gridPos)
	{
		foreach(var player in ContainedPlayers)
		{
			if(player.IsAiming && player.AimingSource == AimingSource.UsingWieldedItem && player.AimingType == AimingType.TargetCell && player.WieldedThing != null)
			{
				if (player.WieldedThing.IsPotentialAimingTargetCell(gridPos))
					player.RefreshWieldedThingTargetAiming();
			}
		}
	}

    int GetDistance(IntVector a, IntVector b)
    {
		var xDiff = MathF.Abs(b.x - a.x);
        var yDiff = MathF.Abs(b.y - a.y);
        return (int)Math.Round(Math.Sqrt(xDiff * xDiff + yDiff * yDiff));
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

    public static float GetDegreesForDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left: return 90f;
            case Direction.Right: return -90f;
            case Direction.Down: return 0f;
            case Direction.Up: return 180f;
        }

        return 0f;
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
        Game.AssertServer();
        return GridThings.TryGetValue(gridPos, out var list) ? list : Enumerable.Empty<Thing>();
    }

    public IEnumerable<Thing> GetThingsAtClient(IntVector gridPos)
    {
        Game.AssertClient();
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
			var currGridPos = new IntVector(Game.Random.Int(0, GridWidth - 1), Game.Random.Int(0, GridHeight - 1));
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
		ContainedPlayers.Clear();
		VisionChangedPlayers.Clear();
    }

	public void AddPlayer(RoguemojiPlayer player)
	{
		ContainedPlayers.Add(player);
	}

	public void RemovePlayer(RoguemojiPlayer player)
	{
		ContainedPlayers.Remove(player);
	}

    // Function to check if there is a clear line of sight between
    // the two given points on a 2D grid
    public bool HasLineOfSight(int x1, int y1, int x2, int y2, float visionStrength, Func<IntVector, float, bool> isSightBlocked)
    {
        // Check if the two points have the same coordinates
        if (x1 == x2 && y1 == y2)
            return true;

        // Calculate the difference between the x and y coordinates
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);

        // Calculate the direction of the line
        int sx = (x1 < x2) ? 1 : -1;
        int sy = (y1 < y2) ? 1 : -1;

        // Check if the line is steep or not
        bool isSteep = (dy > dx);
        if (isSteep)
        {
            // Swap the x and y coordinates if the line is steep
            int temp = x1;
            x1 = y1;
            y1 = temp;

            temp = x2;
            x2 = y2;
            y2 = temp;

            // Recalculate the difference between the x and y coordinates
            dx = Math.Abs(x2 - x1);
            dy = Math.Abs(y2 - y1);
        }

        // Calculate the error value
        int error = dx / 2;

        // Calculate the coordinates of the first point on the line
        int x = x1;
        int y = y1;

        // Loop through the points on the line
        for (int i = 0; i <= dx; i++)
        {
            // Check if there is an obstacle at the current point
            if (isSightBlocked(new IntVector(x, y), visionStrength))
                return false;

            // Update the error value
            error -= dy;
            if (error < 0)
            {
                // Move to the next point on the line
                y += sy;
                error += dx;
            }

            // Move to the next point on the line
            x += sx;
        }

        // There is a clear line of sight between the two given points
        return true;
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
