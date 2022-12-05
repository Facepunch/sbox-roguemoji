using System;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public class PanelFlickerData
{
    public Panel panel;
    public int numFrames;

    public PanelFlickerData(Panel _panel)
    {
        panel = _panel;
        numFrames = 0;
    }
}

public enum LevelId
{
	None,
	Forest0, Forest1, Forest2, Forest3,
}

public partial class RoguemojiGame : Sandbox.Game
{
	public static RoguemojiGame Instance { get; private set; }

	public static int PlayerNum { get; set; }
	public static int ThingId { get; set; }

	public Hud Hud { get; private set; }

	public const int ArenaWidth = 29;
	public const int ArenaHeight = 19;
	public const int InventoryWidth = 10;
	public const int InventoryHeight = 6;
    public const int EquipmentWidth = 4;
    public const int EquipmentHeight = 2;

    public int LevelWidth { get; set; }
	public int LevelHeight { get; set; }

	public record struct LogData(string text, int playerNum);
	public Queue<LogData> LogMessageQueue = new Queue<LogData>();

	public List<RoguemojiPlayer> Players = new List<RoguemojiPlayer>();

	public RoguemojiPlayer LocalPlayer => Local.Client.Pawn as RoguemojiPlayer; // Client-only

	public List<PanelFlickerData> _panelsToFlicker;

    [Net] public IDictionary<LevelId, Level> Levels { get; private set; }

    public RoguemojiGame()
	{
		Instance = this;

		if (Host.IsServer)
		{
            Levels = new Dictionary<LevelId, Level>();

			CreateLevel(LevelId.Forest0);
		}

		if (Host.IsClient)
		{
			Hud = new Hud();
			_panelsToFlicker = new List<PanelFlickerData>();
		}
	}

    [Event.Tick.Server]
	public void ServerTick()
	{
		float dt = Time.Delta;

		HashSet<LevelId> occupiedLevelIds = new();

		foreach(RoguemojiPlayer player in Players)
		{
			if(player != null && player.IsValid)
				occupiedLevelIds.Add(player.CurrentLevelId);
		}

		foreach(var levelId in occupiedLevelIds)
		{
			Level level = Levels[levelId];
			level.Update(dt);
		}
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		if (Hud.MainPanel.LogPanel != null)
		{
			while (LogMessageQueue.Count > 0)
			{
				var data = LogMessageQueue.Dequeue();
				Hud.MainPanel.LogPanel.WriteMessage(data.text, data.playerNum);
			}
		}

        for (int i = _panelsToFlicker.Count - 1; i >= 0; i--)
        {
            var data = _panelsToFlicker[i];
            data.numFrames++;

            if (data.numFrames >= 2)
            {
                if (data.panel != null)
                    data.panel.Style.PointerEvents = PointerEvents.All;

                _panelsToFlicker.RemoveAt(i);
            }
        }
    }

	public override void ClientJoined(Client client) // Server-only
	{
		base.ClientJoined(client);

		var level0 = Levels[LevelId.Forest0];

        level0.GridManager.GetRandomEmptyGridPos(out var gridPos);
        RoguemojiPlayer player = level0.GridManager.SpawnThing<RoguemojiPlayer>(gridPos);
		player.CurrentLevelId = LevelId.Forest0;
		player.PlayerNum = ++PlayerNum;

        var middleCell = new IntVector(MathX.FloorToInt((float)ArenaWidth / 2f), MathX.FloorToInt((float)ArenaHeight / 2f));
		player.SetCameraGridOffset(gridPos - middleCell);

		client.Pawn = player;

		Players.Add(player);
    }

	public override void ClientDisconnect(Client client, NetworkDisconnectionReason reason)
	{
		var player = client.Pawn as RoguemojiPlayer;

		var level = Levels[player.CurrentLevelId];
		level.GridManager.RemoveThing(player);

		// todo: drop or remove items in player's inventory

		Players.Remove(player);

		base.ClientDisconnect(client, reason);
	}

    public void LogMessage(string text, int playerNum)
	{
		LogMessageClient(text, playerNum);
	}

	[ClientRpc]
	public void LogMessageClient(string text, int playerNum)
	{
		if (Hud.MainPanel.LogPanel == null)
		{
			LogMessageQueue.Enqueue(new LogData(text, playerNum));
			return;
		}

		Hud.MainPanel.LogPanel.WriteMessage(text, playerNum);
	}

	[ConCmd.Server]
	public static void GridCellClickedCmd(int x, int y, bool rightClick, bool shift, GridType gridType)
	{
		var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
		Instance.GridCellClicked(new IntVector(x, y), player, rightClick, shift, gridType);
	}

    public void GridCellClicked(IntVector gridPos, RoguemojiPlayer player, bool rightClick, bool shift, GridType gridType)
	{
        Log.Info("Game:GridCellClicked: " + gridPos + ", " + gridType + " rightClick: " + rightClick + " shift: " + shift);

        if(gridType == GridType.Arena)
        {
            var level = Levels[player.CurrentLevelId];
            var thing = level.GridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (!rightClick)
                player.SelectThing(thing);
        }
        else if(gridType == GridType.Inventory)
        {
            var thing = player.InventoryGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (!rightClick)
            {
                if (thing != null && shift)
                    MoveThingToArena(thing, player.GridPos, player);
                else
                    player.SelectThing(thing);
            }
            else
            {
                if (thing != null && thing.Flags.HasFlag(ThingFlags.Equipment))
                {
                    if (player.EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                        MoveThingToEquipment(thing, emptyGridPos, player);
                }
                else
                {
                    player.WieldThing(thing);
                }
            }
        }
        else if(gridType == GridType.Equipment)
        {
            var thing = player.EquipmentGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            Log.Info("thing: " + thing + " #things: " + player.EquipmentGridManager.GetThingsAt(gridPos).Count());
            player.EquipmentGridManager.PrintGridThings();

            if (!rightClick)
            {
                if (thing != null && shift)
                    MoveThingToArena(thing, player.GridPos, player);
                else
                    player.SelectThing(thing);
            }
            else
            {
                if (thing != null && player.InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                {
                    Log.Info("emptyGridPos: " + emptyGridPos);
                    MoveThingToInventory(thing, emptyGridPos, player);
                }
            }
        }
	}

    public void MoveThingToArena(Thing thing, IntVector gridPos, RoguemojiPlayer player)
	{
        if (player.IsDead)
            return;

		var gridManager = Levels[player.CurrentLevelId].GridManager;
        Assert.True(thing.ContainingGridManager != gridManager);

		thing.ContainingGridManager?.RemoveThing(thing);
		RefreshGridPanelClient(To.Single(player), gridType: GridType.Inventory);
        RefreshNearbyPanelClient(To.Single(player));

		thing.InventoryPlayer = null;
        gridManager.AddThing(thing);
		thing.SetGridPos(gridPos);

        if (player.WieldedThing == thing)
            player.WieldThing(null);

        LogMessage(player.DisplayIcon + "(" + player.DisplayName + ") dropped " + thing.DisplayIcon, player.PlayerNum);
    }

	[ClientRpc]
	public void RefreshGridPanelClient(GridType gridType)
	{
		GridPanel panel = Hud.Instance.GetGridPanel(gridType);

		if(panel != null)
			panel.StateHasChanged();
	}

    [ConCmd.Server]
    public static void NearbyThingClickedCmd(int networkIdent, bool rightClick, bool shift)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;

		if (thing.ContainingGridManager.GridType != GridType.Arena)
		{
			Log.Info("Trying to pick up " + thing.Name + " but it's no longer on the ground!");
            return;
        }

        Instance.NearbyThingClicked(thing, rightClick, player, shift);
    }

	public void NearbyThingClicked(Thing thing, bool rightClick, RoguemojiPlayer player, bool shift)
	{
        if (shift || rightClick)
		{
            if (player.InventoryGridManager.GetFirstEmptyGridPos(out var gridPos))
                MoveThingToInventory(thing, gridPos, player);
        }
		else
		{
            player.SelectThing(thing);
        }
    }

    public void MoveThingToInventory(Thing thing, IntVector gridPos, RoguemojiPlayer player)
	{
        if (player.IsDead)
            return;

        Assert.True(!(thing.ContainingGridManager.GridType == GridType.Inventory) || thing.InventoryPlayer != player);

		bool fromNearby = thing.ContainingGridManager.GridType == GridType.Arena;

		thing.ContainingGridManager?.RemoveThing(thing);

		if(fromNearby)
		{
            RefreshNearbyPanelClient(To.Single(player));
            FlickerNearbyPanelCellsClient(To.Single(player));
        }

        thing.InventoryPlayer = player;
        player.InventoryGridManager.AddThing(thing);
		thing.SetGridPos(gridPos);

        LogMessage(player.DisplayIcon + "(" + player.DisplayName + ") picked up " + thing.DisplayIcon, player.PlayerNum);
    }

    public void MoveThingToEquipment(Thing thing, IntVector gridPos, RoguemojiPlayer player)
    {
        if (player.IsDead)
            return;

        if (thing.ContainingGridManager.GridType == GridType.Equipment)
        {
            Log.Error(thing.DisplayName + " at " + gridPos + " is already equipped by " + player.DisplayName + "!");
            return;
        }

        bool fromNearby = thing.ContainingGridManager.GridType == GridType.Arena;

        thing.ContainingGridManager?.RemoveThing(thing);

        if (fromNearby)
        {
            RefreshNearbyPanelClient(To.Single(player));
            FlickerNearbyPanelCellsClient(To.Single(player));
        }

        thing.InventoryPlayer = player;
        player.EquipmentGridManager.AddThing(thing);
        thing.SetGridPos(gridPos);

        LogMessage(player.DisplayIcon + "(" + player.DisplayName + ") equipped " + thing.DisplayIcon, player.PlayerNum);
    }

    public void ChangeInventoryPos(Thing thing, IntVector targetGridPos, RoguemojiPlayer player)
	{
        if (player.IsDead)
            return;

        IntVector currGridPos = thing.GridPos;
		GridManager inventoryGridManager = player.InventoryGridManager;
        Thing otherThing = inventoryGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        inventoryGridManager.DeregisterGridPos(thing, thing.GridPos);
        thing.SetGridPos(targetGridPos);

        if(otherThing != null)
		{
            inventoryGridManager.DeregisterGridPos(otherThing, otherThing.GridPos);
			otherThing.SetGridPos(currGridPos);
        }

        RefreshGridPanelClient(To.Single(player), gridType: GridType.Inventory);
    }

    public void ChangeEquipmentPos(Thing thing, IntVector targetGridPos, RoguemojiPlayer player)
    {
        if (player.IsDead)
            return;

        IntVector currGridPos = thing.GridPos;
        GridManager equipmentGridManager = player.EquipmentGridManager;
        Thing otherThing = equipmentGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        equipmentGridManager.DeregisterGridPos(thing, thing.GridPos);
        thing.SetGridPos(targetGridPos);

        if (otherThing != null)
        {
            equipmentGridManager.DeregisterGridPos(otherThing, otherThing.GridPos);
            otherThing.SetGridPos(currGridPos);
        }

        RefreshGridPanelClient(To.Single(player), gridType: GridType.Equipment);
    }

    [ClientRpc]
    public void RefreshNearbyPanelClient()
    {
		Hud.Instance.MainPanel.NearbyPanel?.StateHasChanged();
    }

    [ClientRpc]
    public void FlickerNearbyPanelCellsClient()
    {
		var nearbyPanel = Hud.Instance.MainPanel?.NearbyPanel;
        if (nearbyPanel == null)
			return;

        nearbyPanel.FlickerCells();
    }

    public void FlickerPanel(Panel panel)
	{
		Host.AssertClient();

		if (panel == null)
			return;

		panel.Style.PointerEvents = PointerEvents.None;
		_panelsToFlicker.Add(new PanelFlickerData(panel));
	}

    [ConCmd.Server]
    public static void InventoryThingDraggedCmd(int networkIdent, PanelType destinationPanelType, int x, int y)
	{
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
		Instance.InventoryThingDragged(thing, destinationPanelType, new IntVector(x, y), player);
    }

    public void InventoryThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos, RoguemojiPlayer player)
	{
        if (destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.Nearby || destinationPanelType == PanelType.None)
		{
            MoveThingToArena(thing, player.GridPos, player);
        }
		else if(destinationPanelType == PanelType.InventoryGrid)
		{
			if (!thing.GridPos.Equals(targetGridPos))
				ChangeInventoryPos(thing, targetGridPos, player);
			else
				player.SelectThing(thing);
		}
        else if (destinationPanelType == PanelType.EquipmentGrid)
		{
            if (!thing.Flags.HasFlag(ThingFlags.Equipment))
                return;

            GridManager equipmentGridManager = player.EquipmentGridManager;
            Thing otherThing = equipmentGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
            IntVector originalGridPos = thing.GridPos;

            MoveThingToEquipment(thing, targetGridPos, player);

            if (otherThing != null)
                MoveThingToInventory(otherThing, originalGridPos, player);
        }
        else if(destinationPanelType == PanelType.Wielding)
        {
            if (player.WieldedThing == thing)
                player.SelectThing(thing);
            else if (!thing.Flags.HasFlag(ThingFlags.Equipment))
                player.WieldThing(thing);
        }
        else if (destinationPanelType == PanelType.PlayerIcon)
        {
            if (thing.Flags.HasFlag(ThingFlags.Equipment))
            {
                if (player.EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    MoveThingToEquipment(thing, emptyGridPos, player);
            }
            else
            {
                player.WieldThing(thing);
            }
        }
    }

    [ConCmd.Server]
    public static void EquipmentThingDraggedCmd(int networkIdent, PanelType destinationPanelType, int x, int y)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
        Instance.EquipmentThingDragged(thing, destinationPanelType, new IntVector(x, y), player);
    }

    public void EquipmentThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos, RoguemojiPlayer player)
    {
        if (destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.Nearby || destinationPanelType == PanelType.None)
        {
            MoveThingToArena(thing, player.GridPos, player);
        }
        else if (destinationPanelType == PanelType.InventoryGrid)
        {
			GridManager inventoryGridManager = player.InventoryGridManager;
			Thing otherThing = inventoryGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
            IntVector originalGridPos = thing.GridPos;

			MoveThingToInventory(thing, targetGridPos, player);

			if (otherThing != null)
			{
				if(otherThing.Flags.HasFlag(ThingFlags.Equipment))
				{
                    MoveThingToEquipment(otherThing, originalGridPos, player);
				}
                else
                {
                    if (player.InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                        ChangeInventoryPos(otherThing, emptyGridPos, player);
                    else
                        MoveThingToArena(otherThing, player.GridPos, player);
                }
			}
		}
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.GridPos.Equals(targetGridPos))
                ChangeEquipmentPos(thing, targetGridPos, player);
            else
                player.SelectThing(thing);
        }
    }

    [ConCmd.Server]
    public static void NearbyThingDraggedCmd(int networkIdent, PanelType destinationPanelType, int x, int y)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
        Instance.NearbyThingDragged(thing, destinationPanelType, new IntVector(x, y), player);
    }

    public void NearbyThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos, RoguemojiPlayer player)
    {
		// dont allow dragging nearby thing from different cells, or if the thing has been picked up by someone else
		if (!player.GridPos.Equals(thing.GridPos) || thing.ContainingGridManager.GridType == GridType.Inventory)
			return;

        if (destinationPanelType == PanelType.InventoryGrid)
        {
            GridManager inventoryGridManager = player.InventoryGridManager;
            Thing otherThing = inventoryGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (otherThing != null)
                MoveThingToArena(otherThing, player.GridPos, player);

            MoveThingToInventory(thing, targetGridPos, player);
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
		{
            if (!thing.Flags.HasFlag(ThingFlags.Equipment))
                return;

            GridManager equipmentGridManager = player.EquipmentGridManager;
            Thing otherThing = equipmentGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (otherThing != null)
                MoveThingToArena(otherThing, player.GridPos, player);

            MoveThingToEquipment(thing, targetGridPos, player);
        }
        else if (destinationPanelType == PanelType.Nearby)
        {
            player.SelectThing(thing);
        }
        else if (destinationPanelType == PanelType.Wielding)
        {
            if (thing.Flags.HasFlag(ThingFlags.Equipment))
                return;

            if (player.InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            {
                MoveThingToInventory(thing, emptyGridPos, player);
                player.WieldThing(thing);
            }
        }
        else if (destinationPanelType == PanelType.PlayerIcon)
        {
            // todo
        }
    }

    [ConCmd.Server]
    public static void WieldingClickedCmd(bool rightClick, bool shift)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Instance.WieldingClicked(player, rightClick, shift);
    }

    public void WieldingClicked(RoguemojiPlayer player, bool rightClick, bool shift)
    {
        if(player.WieldedThing == null) 
            return;

        if(rightClick)
            player.WieldThing(null);
        else if(shift)
            MoveThingToArena(player.WieldedThing, player.GridPos, player);
        else
            player.SelectThing(player.WieldedThing);
    }

    [ConCmd.Server]
    public static void PlayerIconClickedCmd(bool rightClick, bool shift)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Instance.PlayerIconClicked(player, rightClick, shift);
    }

    public void PlayerIconClicked(RoguemojiPlayer player, bool rightClick, bool shift)
    {
        player.SelectThing(player);
    //if (rightClick)
    //    player.WieldThing(null);
    //else if (shift)
    //    MoveThingToArena(player.WieldingThing, player.GridPos, player);
    //else
    //    player.SelectThing(player.WieldingThing);
    }

    public RoguemojiPlayer GetClosestPlayer(IntVector gridPos)
	{
		int closestDistance = int.MaxValue;
		RoguemojiPlayer closestPlayer = null;

		foreach (var player in Players) 
		{
			if (!player.IsValid)
				continue;

			int dist = (player.GridPos - gridPos).ManhattanLength;
			if(dist < closestDistance )
			{
				closestDistance = dist;
				closestPlayer = player;
			}
		}

		return closestPlayer;
	}

	public void Restart()
	{
		foreach(var pair in Levels)
			pair.Value.Restart();

		foreach (RoguemojiPlayer player in Players)
		{
			player.Restart();

			SpawnPlayerOnLevel(player, LevelId.Forest0);
		}
    }

	public void SetPlayerLevel(RoguemojiPlayer player, LevelId levelId)
	{
		if(player.CurrentLevelId != LevelId.None)
		{
			var oldLevel = Levels[player.CurrentLevelId];
			oldLevel.GridManager.RemoveThing(player);

            LogMessage(player.Client.Name + " entered " + levelId, player.PlayerNum);
        }

		SpawnPlayerOnLevel(player, levelId);
    }

	void SpawnPlayerOnLevel(RoguemojiPlayer player, LevelId levelId)
	{
		var level = Levels.ContainsKey(levelId) ? Levels[levelId] : CreateLevel(levelId);
        level.GridManager.AddThing(player);
        player.CurrentLevelId = levelId;
        level.GridManager.GetRandomEmptyGridPos(out var gridPos);
        player.SetGridPos(gridPos);
        var middleCell = new IntVector(MathX.FloorToInt((float)ArenaWidth / 2f), MathX.FloorToInt((float)ArenaHeight / 2f));
        player.SetCameraGridOffset(gridPos - middleCell);
    }

	Level CreateLevel(LevelId levelId)
	{
        var level = new Level();
        level.Init(levelId);
        Levels.Add(levelId, level);

		return level;
    }
}
