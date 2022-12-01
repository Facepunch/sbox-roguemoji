using System;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Interfacer;

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

public partial class InterfacerGame : Sandbox.Game
{
	public static InterfacerGame Instance { get; private set; }

	public static int PlayerNum { get; set; }
	public static int ThingId { get; set; }

	public Hud Hud { get; private set; }

	[Net] public GridManager ArenaGridManager { get; private set; }

	public const int ArenaWidth = 29;
	public const int ArenaHeight = 19;
	public const int InventoryWidth = 10;
	public const int InventoryHeight = 6;

	public int LevelWidth { get; set; }
	public int LevelHeight { get; set; }

	public record struct LogData(string text, int playerNum);
	public Queue<LogData> LogMessageQueue = new Queue<LogData>();

	public List<InterfacerPlayer> Players = new List<InterfacerPlayer>();

	public InterfacerPlayer LocalPlayer => Local.Client.Pawn as InterfacerPlayer; // Client-only

	public List<PanelFlickerData> _panelsToFlicker;


	public InterfacerGame()
	{
		Instance = this;

		if (Host.IsServer)
		{
			LevelWidth = 40;
			LevelHeight = 25;
			ArenaGridManager = new();
			ArenaGridManager.Init(LevelWidth, LevelHeight);

			SpawnStartingThings();
		}

		if (Host.IsClient)
		{
			Hud = new Hud();
			_panelsToFlicker = new List<PanelFlickerData>();
		}
	}

	void SpawnStartingThings()
	{
        for (int x = 0; x < LevelWidth; x++)
        {
            ArenaGridManager.SpawnThing<OilBarrel>(new IntVector(x, 0));
            ArenaGridManager.SpawnThing<OilBarrel>(new IntVector(x, LevelHeight - 1));
        }

        for (int y = 1; y < LevelHeight - 1; y++)
        {
            ArenaGridManager.SpawnThing<OilBarrel>(new IntVector(0, y));
            ArenaGridManager.SpawnThing<OilBarrel>(new IntVector(LevelWidth - 1, y));
        }

        ArenaGridManager.SpawnThing<Rock>(new IntVector(10, 10));
        ArenaGridManager.SpawnThing<Leaf>(new IntVector(9, 10));
        ArenaGridManager.SpawnThing<Leaf>(new IntVector(21, 19));

        for (int i = 0; i < 20; i++)
        {
            if (ArenaGridManager.GetRandomEmptyGridPos(out var gridPos))
                ArenaGridManager.SpawnThing<TreeEvergreen>(gridPos);
        }

        for (int i = 0; i < 5; i++)
        {
            if (ArenaGridManager.GetRandomEmptyGridPos(out var gridPos))
                ArenaGridManager.SpawnThing<Squirrel>(gridPos);
        }
    }

    [Event.Tick.Server]
	public void ServerTick()
	{
		if (ArenaGridManager == null)
			return;

		float dt = Time.Delta;
		
		ArenaGridManager.Update(dt);
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

		ArenaGridManager.GetRandomEmptyGridPos(out var gridPos);
        InterfacerPlayer player = ArenaGridManager.SpawnThing<InterfacerPlayer>(gridPos);
		player.PlayerNum = ++PlayerNum;

        var middleCell = new IntVector(MathX.FloorToInt((float)ArenaWidth / 2f), MathX.FloorToInt((float)ArenaHeight / 2f));
		player.SetCameraGridOffset(gridPos - middleCell);

		client.Pawn = player;

		Players.Add(player);
    }

	public override void ClientDisconnect(Client client, NetworkDisconnectionReason reason)
	{
		var player = client.Pawn as InterfacerPlayer;
		ArenaGridManager.RemoveThing(player);

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
	public static void CellClickedArenaCmd(int x, int y, bool rightClick, bool shift)
	{
		var player = ConsoleSystem.Caller.Pawn as InterfacerPlayer;
		Instance.CellClickedArena(new IntVector(x, y), player, rightClick, shift);
	}

	[ConCmd.Server]
	public static void CellClickedInventoryCmd(int x, int y, bool rightClick, bool shift)
	{
		var player = ConsoleSystem.Caller.Pawn as InterfacerPlayer;
		Instance.CellClickedInventory(new IntVector(x, y), player, rightClick, shift);
	}

	public void CellClickedArena(IntVector gridPos, InterfacerPlayer player, bool rightClick, bool shift)
	{
		var thing = ArenaGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        //LogMessage(player.Client.Name + (shift ? " shift-" : " ") + (rightClick ? "right-clicked " : "clicked ") + (thing != null ? (thing.DisplayIcon + " at ") : "") + gridPos + ".", player.PlayerNum);

        if (!rightClick)
			player.SelectThing(thing);

  //      if (thing == null)
  //      {
		//	if (Rand.Float(0f, 1f) < 0.1f)
		//	{
		//		var rock = Instance.SpawnThingArena(TypeLibrary.GetDescription(typeof(Rock)), gridPos);
		//		LogMessage(player.Client.Name + " created " + rock.DisplayIcon + " at " + gridPos + "!", player.PlayerNum);
		//	}
		//	else
  //          {
  //              var explosion = Instance.SpawnThingArena(TypeLibrary.GetDescription(typeof(Explosion)), gridPos);
  //              explosion.VfxShake(0.15f, 4f);
		//		explosion.VfxScale(0.15f, Rand.Float(0.6f, 0.8f), Rand.Float(0.3f, 0.4f));
		//	}
		//}
	}

	public void CellClickedInventory(IntVector gridPos, InterfacerPlayer player, bool rightClick, bool shift)
	{
		var thing = player.InventoryGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        //LogMessage(player.Client.Name + (shift ? " shift-" : " ") + (rightClick ? "right-clicked " : "clicked ") + (thing != null ? (thing.DisplayIcon + " at ") : "") + gridPos + " in their inventory.", player.PlayerNum);

        if (!rightClick && thing != null)
		{
			if(shift)
            {
                MoveThingToArena(thing, player.GridPos, player);
            }
			else
			{
                player.SelectThing(thing);
            }
		}
	}

    public Thing SpawnThingInventory(TypeDescription type, IntVector gridPos, InterfacerPlayer player)
	{
        Host.AssertServer();

        var thing = type.Create<Thing>();
		thing.Flags |= ThingFlags.InInventory;
		thing.InventoryPlayer = player;
		player.InventoryGridManager.AddThing(thing);
		thing.SetGridPos(gridPos);

        return thing;
	}

    public T SpawnThingInventory<T>(IntVector gridPos, InterfacerPlayer player) where T : Thing
    {
		return SpawnThingInventory(TypeLibrary.GetDescription(typeof(T)), gridPos, player) as T;
    }

    public void MoveThingToArena(Thing thing, IntVector gridPos, InterfacerPlayer player)
	{
        Assert.True(thing.ContainingGridManager != ArenaGridManager);

		thing.ContainingGridManager?.RemoveThing(thing);
		RefreshGridPanelClient(To.Single(player), inventory: true);
        RefreshNearbyPanelClient(To.Single(player));

        thing.Flags &= ~ThingFlags.InInventory;
		thing.InventoryPlayer = null;
		ArenaGridManager.AddThing(thing);
		thing.SetGridPos(gridPos);

        LogMessage(player.DisplayIcon + "(" + player.DisplayName + ") dropped " + thing.DisplayIcon, player.PlayerNum);
    }

	[ClientRpc]
	public void RefreshGridPanelClient(bool inventory)
	{
		GridPanel panel = inventory ? Hud.Instance.MainPanel.InventoryPanel : Hud.Instance.MainPanel.ArenaPanel;
		panel.StateHasChanged();
	}

    [ConCmd.Server]
    public static void NearbyThingClickedCmd(int networkIdent, bool rightClick, bool shift)
    {
        var player = ConsoleSystem.Caller.Pawn as InterfacerPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;

		if (thing.Flags.HasFlag(ThingFlags.InInventory))
		{
			Log.Info(thing.Name + " " + thing.NetworkIdent + "!!!!");
            return;
        }

        Instance.NearbyThingClicked(thing, rightClick, player, shift);
    }

	public void NearbyThingClicked(Thing thing, bool rightClick, InterfacerPlayer player, bool shift)
	{
        //LogMessage(player.Client.Name + (shift ? " shift-" : " ") + (rightClick ? "right-clicked " : "clicked ") + thing.DisplayIcon + " nearby them.", player.PlayerNum);

        if (shift || rightClick)
		{
            IntVector gridPos;
            if (player.InventoryGridManager.GetFirstEmptyGridPos(out gridPos))
            {
                MoveThingToInventory(thing, gridPos, player);
            }
        }
		else
		{
            player.SelectThing(thing);
        }
    }

    public void MoveThingToInventory(Thing thing, IntVector gridPos, InterfacerPlayer player)
	{
        if (thing.Flags.HasFlag(ThingFlags.InInventory))
		{
			Log.Error(thing.DisplayName + " at " + gridPos + " is already in inventory of " + player.DisplayName + "!");
		}

        if (thing.InventoryPlayer == player)
        {
            Log.Error(thing.DisplayName + " has same InventoryPlayer!");
        }

        Assert.True(!thing.Flags.HasFlag(ThingFlags.InInventory) || thing.InventoryPlayer != player);

		thing.ContainingGridManager?.RemoveThing(thing);
		RefreshNearbyPanelClient(To.Single(player));
        FlickerNearbyPanelCellsClient(To.Single(player));

        thing.InventoryPlayer = player;
        thing.Flags |= ThingFlags.InInventory;
        player.InventoryGridManager.AddThing(thing);
		thing.SetGridPos(gridPos);

        LogMessage(player.DisplayIcon + "(" + player.DisplayName + ") picked up " + thing.DisplayIcon, player.PlayerNum);
    }

	public void ChangeInventoryPos(Thing thing, IntVector targetGridPos, InterfacerPlayer player)
	{
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

        RefreshGridPanelClient(To.Single(player), inventory: true);
    }

    [ClientRpc]
    public void RefreshNearbyPanelClient()
    {
		Hud.Instance.MainPanel.NearbyPanel.StateHasChanged();
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
        var player = ConsoleSystem.Caller.Pawn as InterfacerPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
		Instance.InventoryThingDragged(thing, destinationPanelType, new IntVector(x, y), player);
    }

    public void InventoryThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos, InterfacerPlayer player)
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
    }

    [ConCmd.Server]
    public static void NearbyThingDraggedCmd(int networkIdent, PanelType destinationPanelType, int x, int y)
    {
        var player = ConsoleSystem.Caller.Pawn as InterfacerPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
        Instance.NearbyThingDragged(thing, destinationPanelType, new IntVector(x, y), player);
    }

    public void NearbyThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos, InterfacerPlayer player)
    {
		// dont allow dragging nearby thing from different cells, or if the thing has been picked up by someone else
		if (!player.GridPos.Equals(thing.GridPos) || thing.Flags.HasFlag(ThingFlags.InInventory))
			return;

        if (destinationPanelType == PanelType.InventoryGrid)
        {
            GridManager inventoryGridManager = player.InventoryGridManager;
            Thing otherThing = inventoryGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (otherThing != null)
                MoveThingToArena(otherThing, player.GridPos, player);

            MoveThingToInventory(thing, targetGridPos, player);
        }
        else if (destinationPanelType == PanelType.Nearby)
        {
            player.SelectThing(thing);
        }
    }

	public InterfacerPlayer GetClosestPlayer(IntVector gridPos)
	{
		int closestDistance = int.MaxValue;
		InterfacerPlayer closestPlayer = null;

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
		ArenaGridManager.Restart();

		SpawnStartingThings();

		foreach (InterfacerPlayer player in Players)
		{
			player.Restart();

			ArenaGridManager.AddThing(player);
            ArenaGridManager.GetRandomEmptyGridPos(out var gridPos);
			var middleCell = new IntVector(MathX.FloorToInt((float)ArenaWidth / 2f), MathX.FloorToInt((float)ArenaHeight / 2f));
			player.GridPos = gridPos;
			player.SetCameraGridOffset(gridPos - middleCell);
		}
    }
}
