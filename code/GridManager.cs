﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Sandbox;

namespace Roguemoji;

public enum Direction { None, Left, Right, Down, Up, LeftDown, RightDown, LeftUp, RightUp }

public enum GridType { None, Arena, Inventory, Equipment }

public partial class GridManager : Entity
{
	[Net] public int GridWidth { get; private set; }
	[Net] public int GridHeight { get; private set; }

    [Net] public IList<Thing> Things { get; private set; }
	[Net] public GridType GridType { get; set; }

    public Dictionary<IntVector, List<Thing>> GridThings = new Dictionary<IntVector, List<Thing>>();

	[Net] public RoguemojiPlayer OwningPlayer { get; set; }

	public HashSet<RoguemojiPlayer> ContainedPlayers = new HashSet<RoguemojiPlayer>();
	public HashSet<RoguemojiPlayer> VisionChangedPlayers = new HashSet<RoguemojiPlayer>(); // players who need an update to their field of view
    public HashSet<RoguemojiPlayer> CheckSeenThingsPlayers = new HashSet<RoguemojiPlayer>(); // players who need to check for unnecessary seen things

    [Net] public LevelId LevelId { get; set; }

    public GridManager()
    {
        if (Game.IsClient)
        {
            Floaters = new List<GridFloaterData>();
        }
    }

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
				player.RefreshVisibility(To.Single(player));

			VisionChangedPlayers.Clear();

            foreach (var player in CheckSeenThingsPlayers)
                player.CheckForUnnecessarySeenThings(To.Single(player));

            CheckSeenThingsPlayers.Clear();
        }
	}

    public void UpdateClient(float dt)
    {
        HandleFloaters(dt);
    }

	void UpdateThings(IList<Thing> things, float dt)
	{
		for (int i = things.Count - 1; i >= 0; i--)
		{
			if(i >= things.Count)
				continue;

			var thing = things[i];

            //thing.DebugText = $"{thing.ContainingGridType}";
            
            // todo: only update if a component needs updating
            if (thing.NeedsUpdate() || (thing.WieldedThing != null && thing.WieldedThing.NeedsUpdate()))
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

		thing.OnSpawned();

        if ((GridType == GridType.Inventory || GridType == GridType.Equipment) && OwningPlayer != null)
            thing.ThingOwningThis = OwningPlayer;

        return thing;
    }

    public void AddThing(Thing thing)
	{
		Things.Add(thing);

        thing.ContainingGridManager = this;
		thing.ContainingGridType = GridType;

        var player = thing as RoguemojiPlayer;
		if (player != null)
            AddPlayer(player);
    }

	public void RemoveThing(Thing thing)
	{
        DeregisterGridPos(thing, thing.GridPos);

		Things.Remove(thing);
		thing.ContainingGridManager = null;
		thing.ContainingGridType = GridType.None;

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
        Sandbox.Diagnostics.Assert.True(IsGridPosInBounds(gridPos));

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
			if (thing.GetStatClamped(StatType.SightBlockAmount) > 0)
				CheckPlayerVisionChange(thing, gridPos, PlayerVisionChangeReason.ChangedGridPos);

			CheckAimingChange(gridPos);

            CheckSeenThingVision(thing, gridPos);
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
				if (thing.GetStatClamped(StatType.SightBlockAmount) > 0)
					CheckPlayerVisionChange(thing, gridPos, PlayerVisionChangeReason.ChangedGridPos);

				CheckAimingChange(gridPos);
			}
        }
    }

	public void CheckPlayerVisionChange(Thing thing, IntVector gridPos, PlayerVisionChangeReason reason)
	{
		foreach(var player in ContainedPlayers)
		{
            if (VisionChangedPlayers.Contains(player) || thing == player)
                continue;

			var sight = player.GetStatClamped(StatType.Sight);

            // if thing doesn't block our sight, we can ignore it, unless it lowered its SightBlockAmount (as it might previously have been blocking us)
            if (thing.GetStatClamped(StatType.SightBlockAmount) < sight && reason != PlayerVisionChangeReason.DecreasedSightBlockAmount)
				continue;

			var dist = GetDistance(player.GridPos, gridPos);
			if (dist > sight)
				continue;

			VisionChangedPlayers.Add(player);
		}
	}

    // something moved that a player might have seen. check whether their previously seen cells are showing an old position of that thing, and if so, remove it if we now see it
    public void CheckSeenThingVision(Thing thing, IntVector gridPos)
    {
        foreach (var player in ContainedPlayers)
        {
            if (CheckSeenThingsPlayers.Contains(player))
                continue;

            var sight = player.GetStatClamped(StatType.Sight);

            var dist = GetDistance(player.GridPos, gridPos);
            if (dist > sight)
                continue;

            CheckSeenThingsPlayers.Add(player);
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

	public void PlayerChangedGridPos(RoguemojiPlayer player)
	{
		foreach(var pair in GridThings)
		{
            foreach (var thing in pair.Value)
                thing.OnPlayerChangedGridPos(player);
        }
	}

    public static int GetDistance(IntVector a, IntVector b)
    {
        if(a.Equals(b)) 
            return 0;

		var xDiff = MathF.Abs(b.x - a.x);
        var yDiff = MathF.Abs(b.y - a.y);
        return (int)Math.Round(Math.Sqrt(xDiff * xDiff + yDiff * yDiff));
    }

    public static IntVector GetIntVectorForDirection(Direction direction)
	{
		switch(direction)
		{
			case Direction.Left:  return new IntVector(-1, 0);
			case Direction.Right: return new IntVector(1, 0);
			case Direction.Down: return new IntVector(0, 1);
			case Direction.Up: return new IntVector(0, -1);
            case Direction.LeftDown: return new IntVector(-1, 1);
            case Direction.RightDown: return new IntVector(1, 1);
            case Direction.LeftUp: return new IntVector(-1, -1);
            case Direction.RightUp: return new IntVector(1, -1);
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
            case Direction.LeftDown: return new Vector2(-1f, 1f);
            case Direction.RightDown: return new Vector2(1f, 1f);
            case Direction.LeftUp: return new Vector2(-1f, -1f);
            case Direction.RightUp: return new Vector2(1f, -1f);
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
            case Direction.LeftDown: return 45f;
            case Direction.RightDown: return -45f;
            case Direction.LeftUp: return 135f;
            case Direction.RightUp: return -135f;
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
		else if (vec.x == -1 && vec.y == 1)
            return Direction.LeftDown;
        else if (vec.x == 1 && vec.y == 1)
            return Direction.RightDown;
        else if (vec.x == -1 && vec.y == -1)
            return Direction.LeftUp;
        else if (vec.x == 1 && vec.y == -1)
            return Direction.RightUp;

        return Direction.None;
    }

    public static Direction GetOppositeDirection(Direction dir)
    {
        switch(dir)
        {
            case Direction.Left: return Direction.Right;
            case Direction.Right: return Direction.Left;
            case Direction.Down: return Direction.Up;
            case Direction.Up: return Direction.Down;
            case Direction.LeftDown: return Direction.RightUp;
            case Direction.RightDown: return Direction.LeftUp;
            case Direction.LeftUp: return Direction.RightDown;
            case Direction.RightUp: return Direction.LeftDown;
        }

        return Direction.None;
    }

    public static IntVector GetIntVectorForSlope(IntVector a, IntVector b)
    {
        int outX = 0;
        int outY = 0;

        int dx = b.x - a.x;
        int dy = b.y - a.y;

        int sx = dx > 0 ? 1 : -1;
        int sy = dy > 0 ? 1 : -1;

        dx = Math.Abs(dx);
        dy = Math.Abs(dy);

        int err = (dx > dy ? dx : -dy) / 2;

        int e2 = err;
        if(e2 > -dx)
        {
            err -= dy;
            outX += sx;
        }

        if(e2 < dy)
        {
            err += dx;
            outY += sy;
        }

        return new IntVector(outX, outY);
    }

    public List<Direction> GetDirectionsInBounds(IntVector startPos, bool cardinalOnly = true)
    {
        List<Direction> directions = new List<Direction>();
		var potentialDirections = cardinalOnly ? GetCardinalDirections() : GetAllDirections();

		foreach(var dir in potentialDirections)
		{
			if(IsGridPosInBounds(startPos + GetIntVectorForDirection(dir)))
				directions.Add(dir);
		}

		return directions;
    }

    public static Direction GetRandomDirection(bool cardinalOnly = true)
    {
        var directions = cardinalOnly ? GetCardinalDirections() : GetAllDirections();
        return directions[Game.Random.Int(0, directions.Count - 1)];
    }

    public static List<Direction> GetCardinalDirections()
	{
		return new List<Direction>() { Direction.Left, Direction.Right, Direction.Down, Direction.Up };
	}

    public static List<Direction> GetAllDirections()
    {
        return new List<Direction>() { Direction.Left, Direction.LeftUp, Direction.Up, Direction.RightUp, Direction.Right, Direction.RightDown, Direction.Down, Direction.LeftDown };
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

    public bool DoesGridPosContainThingType(IntVector gridPos, TypeDescription type)
    {
        var things = GetThingsAt(gridPos);

        foreach(Thing thing in things)
        {
            if (type == TypeLibrary.GetType(thing.GetType()))
                return true;
        }

        return false;
    }

    public bool DoesGridPosContainThingType<T>(IntVector gridPos) where T : Thing
    {
        return DoesGridPosContainThingType(gridPos, TypeLibrary.GetType(typeof(T)));
    }

    public IEnumerable<Thing> GetThingsWithinRange(IntVector gridPos, int range, ThingFlags allFlags = ThingFlags.None, ThingFlags anyFlags = ThingFlags.None, ThingFlags noneFlags = ThingFlags.None)
	{
		var things = new List<Thing>();

		foreach((IntVector cellPos, List<Thing> cellThings) in GridThings)
		{
			if(Utils.GetDistance(gridPos, cellPos) <= range)
			{
				foreach(var cellThing in cellThings)
				{
					if( (allFlags == ThingFlags.None || (cellThing.Flags & allFlags) == allFlags) && 
						(anyFlags == ThingFlags.None || (cellThing.Flags & anyFlags) != 0) && 
						(noneFlags == ThingFlags.None || (cellThing.Flags & noneFlags) == 0))
						things.Add(cellThing);
                }
			}
		}

		return things;
	}

    public IEnumerable<Thing> GetAllThings(ThingFlags allFlags = ThingFlags.None, ThingFlags anyFlags = ThingFlags.None, ThingFlags noneFlags = ThingFlags.None)
    {
        var things = new List<Thing>();

        foreach ((IntVector cellPos, List<Thing> cellThings) in GridThings)
        {
            foreach (var cellThing in cellThings)
            {
                if ((allFlags == ThingFlags.None || (cellThing.Flags & allFlags) == allFlags) &&
                    (anyFlags == ThingFlags.None || (cellThing.Flags & anyFlags) != 0) &&
                    (noneFlags == ThingFlags.None || (cellThing.Flags & noneFlags) == 0))
                    things.Add(cellThing);
            }
        }

        return things;
    }

    public float GetPathfindMovementCost(IntVector gridPos)
	{
		float movementCost = 0f;
		var things = GetThingsAt(gridPos).WithAny(ThingFlags.Solid | ThingFlags.Exclusive);
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

	public bool GetRandomEmptyGridPos(out IntVector gridPos, bool allowNonSolid = false)
	{
		HashSet<int> gridIndexes = new();
		for (int x = 4; x < GridWidth; x++)
			for (int y = 4; y < GridHeight; y++)
				gridIndexes.Add(GetIndex(x, y, GridWidth));

		while(gridIndexes.Count > 0)
		{
			int index = Game.Random.Int(0, gridIndexes.Count - 1);
			var currGridPos = GetGridPos(index);

            var things = allowNonSolid ? GetThingsAt(currGridPos).WithAny(ThingFlags.Solid | ThingFlags.Exclusive) : GetThingsAt(currGridPos);
            if (things.Count() == 0)
            {
                gridPos = currGridPos;
                return true;
            }

			gridIndexes.Remove(index);
        }

        gridPos = IntVector.Zero;
        return false;
    }

    public bool GetRandomEmptyAdjacentGridPos(IntVector startGridPos, out IntVector gridPos, bool allowNonSolid = false, bool cardinalOnly = false)
	{
        List<IntVector> gridPositions = new();
        gridPos = startGridPos;

        for (int x = -1; x <= 1; x++)
		{
            for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
					continue;

				var currGridPos = startGridPos + new IntVector(x, y);
				if(!IsGridPosInBounds(currGridPos))
					continue;

                var things = allowNonSolid ? GetThingsAt(currGridPos).WithAny(ThingFlags.Solid | ThingFlags.Exclusive) : GetThingsAt(currGridPos);

                if (things.Count() == 0)
					gridPositions.Add(currGridPos);
            }
        }

		if(gridPositions.Count > 0)
		{
			gridPos = gridPositions[Game.Random.Int(0, gridPositions.Count - 1)];
			return true;
		}

        return false;
    }

    public bool GetRandomAdjacentGridPos(IntVector startGridPos, out IntVector gridPos, bool cardinalOnly = false)
    {
        List<IntVector> gridPositions = new();
        gridPos = startGridPos;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                var currGridPos = startGridPos + new IntVector(x, y);
                if (!IsGridPosInBounds(currGridPos))
                    continue;
                
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

    public bool GetRandomEmptyGridPosWithinRange(IntVector startGridPos, out IntVector gridPos, int range, bool allowNonSolid = false)
    {
        List<IntVector> gridPositions = new();
        gridPos = startGridPos;

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                var currGridPos = startGridPos + new IntVector(x, y);
                if (!IsGridPosInBounds(currGridPos))
                    continue;

                if (Utils.GetDistance(startGridPos, currGridPos) > range)
                    continue;

                var things = allowNonSolid ? GetThingsAt(currGridPos).WithAny(ThingFlags.Solid | ThingFlags.Exclusive) : GetThingsAt(currGridPos);
                if (things.Count() > 0)
                    continue;

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

    public static bool IsAdjacent(IntVector a, IntVector b, bool cardinalOnly = false)
    {
        return (a - b).ManhattanLength <= (cardinalOnly ? 1 : 2);
    }

    public void Restart()
	{
        foreach (var thing in Things)
        {
            if (thing is not RoguemojiPlayer)
                thing.CleanUpAndDelete();
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

	public void SetWidth(int width)
	{
		if (width == GridWidth || width < 1)
			return;

		bool isReducingSize = width < GridWidth;

        GridWidth = width;

		if(isReducingSize)
		{
            foreach (var pair in GridThings)
            {
                if(!IsGridPosInBounds(pair.Key))
				{
					if(GridType == GridType.Inventory && OwningPlayer != null)
					{
						var things = pair.Value;
						for(int i = things.Count - 1; i >= 0; i--)
						{
							var thing = things[i];
                            OwningPlayer.MoveThingTo(thing, GridType.Arena, OwningPlayer.GridPos, dontRequireAction: true);
                        }
					}
				}
            }
        }
	}

	public string GetNearbyBgColor(IntVector gridPos)
	{
		return Level.GetLevelBgColor(LevelId, Utils.IsOdd(gridPos));
	}

    public bool HasLineOfSight(IntVector gridPosA, IntVector gridPosB, int sight, out IntVector collisionCell)
    {
		int x1 = gridPosA.x;
		int y1 = gridPosA.y;
		int x2 = gridPosB.x;
		int y2 = gridPosB.y;

        collisionCell = gridPosB;

		if (gridPosA.Equals(gridPosB))
			return true;

        // check if nearby and visible to prevent false negative
        IntVector diff = gridPosB - gridPosA;
		if(diff.ManhattanLength == 3 && diff.x != 0 && diff.y != 0)
		{
			// test the 2 squares that would allow you to see target
			IntVector cellA = gridPosA + (Math.Abs(diff.x) > Math.Abs(diff.y) ? new IntVector(Math.Sign(diff.x), 0) : new IntVector(0, Math.Sign(diff.y)));
			if(!BlocksSight(cellA, sight))
                return true;

			IntVector cellB = gridPosA + new IntVector(Math.Sign(diff.x), Math.Sign(diff.y));
			if(BlocksSight(cellB, sight)) 
			{
                collisionCell = cellB;
                //RoguemojiGame.Instance.DebugGridCell(cellB, new Color(1f, 0f, 1f, 0.3f), 0.05f, LevelId);
                return false;
            }
			else
			{
				return true;
			}
        }

        int w = x2 - x1;
        int h = y2 - y1;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            var currGridPos = new IntVector(x1, y1);
			if(!currGridPos.Equals(gridPosA) && !currGridPos.Equals(gridPosB))
			{
				if(BlocksSight(currGridPos, sight))
				{
                    collisionCell = currGridPos;
                    //RoguemojiGame.Instance.DebugGridCell(currGridPos, new Color(1f, 0f, 0f, 0.3f), 0.05f, LevelId);
                    return false;
                }

                //RoguemojiGame.Instance.DebugGridCell(currGridPos, new Color(0f, 0f, 1f, 0.3f), 0.05f, LevelId);
            }

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x1 += dx1;
                y1 += dy1;
            }
            else
            {
                x1 += dx2;
                y1 += dy2;
            }
        }

        return true;
    }

	bool BlocksSight(IntVector gridPos, int sight)
	{
        foreach (var thing in GetThingsAt(gridPos))
        {
            if (thing.GetStatClamped(StatType.SightBlockAmount) >= sight && thing.GetStatClamped(StatType.Invisible) <= 0)
				return true;
        }

		return false;
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

    public HashSet<IntVector> GetPointsOnCircle(IntVector center, int radius)
    {
        HashSet<IntVector> points = new HashSet<IntVector>();

        int x = radius;
        int y = 0;
        int decisionOver2 = 1 - x;   // Decision criterion divided by 2 evaluated at x=r, y=0

        while (y <= x)
        {
            if (IsGridPosInBounds(new IntVector(center.x + x, center.y + y))) points.Add(new IntVector(center.x + x, center.y + y)); // Octant 1
            if (IsGridPosInBounds(new IntVector(center.x - x, center.y + y))) points.Add(new IntVector(center.x - x, center.y + y)); // Octant 4
            if (IsGridPosInBounds(new IntVector(center.x + x, center.y - y))) points.Add(new IntVector(center.x + x, center.y - y)); // Octant 8
            if (IsGridPosInBounds(new IntVector(center.x - x, center.y - y))) points.Add(new IntVector(center.x - x, center.y - y)); // Octant 5
            if (IsGridPosInBounds(new IntVector(center.x + y, center.y + x))) points.Add(new IntVector(center.x + y, center.y + x)); // Octant 2
            if (IsGridPosInBounds(new IntVector(center.x - y, center.y + x))) points.Add(new IntVector(center.x - y, center.y + x)); // Octant 3
            if (IsGridPosInBounds(new IntVector(center.x + y, center.y - x))) points.Add(new IntVector(center.x + y, center.y - x)); // Octant 7
            if (IsGridPosInBounds(new IntVector(center.x - y, center.y - x))) points.Add(new IntVector(center.x - y, center.y - x)); // Octant 6

            y++;
            if (decisionOver2 <= 0)
            {
                decisionOver2 += 2 * y + 1;
            }
            else
            {
                x--;
                decisionOver2 += 2 * (y - x) + 1;
            }
        }

        return points;
    }

    public HashSet<IntVector> GetPointsWithinCircle(IntVector center, int radius)
    {
        HashSet<IntVector> points = new HashSet<IntVector>();

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y < radius * radius + radius)
                {
                    if (IsGridPosInBounds(new IntVector(center.x + x, center.y + y))) points.Add(new IntVector(center.x + x, center.y + y));
                }
            }
        }

        return points;
    }
}
