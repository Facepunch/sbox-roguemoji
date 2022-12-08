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

public partial class RoguemojiGame : GameManager
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
	public static void GridCellClickedCmd(int x, int y, GridType gridType, bool rightClick, bool shift, bool doubleClick, bool visible = true)
	{
		var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
		player.GridCellClicked(new IntVector(x, y), gridType, rightClick, shift, doubleClick, visible);
	}

	[ClientRpc]
	public void RefreshGridPanelClient(GridType gridType)
	{
		GridPanel panel = Hud.Instance.GetGridPanel(gridType);

		if(panel != null)
			panel.StateHasChanged();
	}

    [ConCmd.Server]
    public static void NearbyThingClickedCmd(int networkIdent, bool rightClick, bool shift, bool doubleClick)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;

		if (thing.ContainingGridManager.GridType != GridType.Arena)
		{
			Log.Info("Trying to pick up " + thing.Name + " but it's no longer on the ground!");
            return;
        }

        player.NearbyThingClicked(thing, rightClick, shift, doubleClick);
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
		player.InventoryThingDragged(thing, destinationPanelType, new IntVector(x, y));
    }

    [ConCmd.Server]
    public static void EquipmentThingDraggedCmd(int networkIdent, PanelType destinationPanelType, int x, int y)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
        player.EquipmentThingDragged(thing, destinationPanelType, new IntVector(x, y));
    }

    [ConCmd.Server]
    public static void NearbyThingDraggedCmd(int networkIdent, PanelType destinationPanelType, int x, int y)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
        player.NearbyThingDragged(thing, destinationPanelType, new IntVector(x, y));
    }

    [ConCmd.Server]
    public static void WieldingClickedCmd(bool rightClick, bool shift)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        player.WieldingClicked(rightClick, shift);
    }

    [ConCmd.Server]
    public static void PlayerIconClickedCmd(bool rightClick, bool shift)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        player.PlayerIconClicked(rightClick, shift);
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
