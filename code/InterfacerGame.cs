using System;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

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

	public const int ArenaWidth = 30;
	public const int ArenaHeight = 20;
	public const int InventoryWidth = 10;
	public const int InventoryHeight = 5;

	public record struct LogData(string text, int playerNum);
	public Queue<LogData> LogMessageQueue = new Queue<LogData>();

	public InterfacerPlayer LocalPlayer => Local.Client.Pawn as InterfacerPlayer; // Client-only

    public List<PanelFlickerData> _panelsToFlicker;

    public InterfacerGame()
	{
		Instance = this;

		if (Host.IsServer)
		{
			ArenaGridManager = new();
			ArenaGridManager.Init(ArenaWidth, ArenaHeight);

			SpawnThingArena(TypeLibrary.GetDescription(typeof(Rock)), new IntVector(10, 10));
            SpawnThingArena(TypeLibrary.GetDescription(typeof(Leaf)), new IntVector(9, 10));
            SpawnThingArena(TypeLibrary.GetDescription(typeof(Leaf)), new IntVector(21, 19));
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

        SpawnThingArena(TypeLibrary.GetDescription(typeof(Leaf)), new IntVector(4, 4));
        
		InterfacerPlayer player = SpawnThingArena(TypeLibrary.GetDescription(typeof(InterfacerPlayer)), new IntVector(5, 10)) as InterfacerPlayer;
		player.PlayerNum = ++PlayerNum;
		client.Pawn = player;

		for(int x = 0; x < InventoryWidth; x++)
		{
            for (int y = 0; y < InventoryHeight; y++)
			{
				SpawnThingInventory(TypeLibrary.GetDescription(GetRandomType()), new IntVector(x, y), player);
            }
        }
		
        SpawnThingArena(TypeLibrary.GetDescription(GetRandomType()), new IntVector(5, 4));
    }

	Type GetRandomType()
	{
		int rand = Rand.Int(0, 6);
		switch (rand)
		{
			case 0: return typeof(Leaf);
			case 1: return typeof(Potato);
			case 2: return typeof(Nut);
            case 3: return typeof(Mushroom);
            case 4: return typeof(Trumpet);
            case 5: return typeof(Bouquet);
            case 6: default: return typeof(Cheese);
		}
	}

	public override void ClientDisconnect(Client client, NetworkDisconnectionReason reason)
	{
		var player = client.Pawn as InterfacerPlayer;
		ArenaGridManager.RemoveThing(player);

		// todo: drop or remove items in player's inventory

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
		var thing = ArenaGridManager.GetThingAt(gridPos, ThingFlags.Selectable);

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
		var thing = player.InventoryGridManager.GetThingAt(gridPos, ThingFlags.Selectable);
        //LogMessage(player.Client.Name + (shift ? " shift-" : " ") + (rightClick ? "right-clicked " : "clicked ") + (thing != null ? (thing.DisplayIcon + " at ") : "") + gridPos + " in their inventory.", player.PlayerNum);

        if (!rightClick)
		{
			if(shift && thing != null)
            {
                MoveThingToArena(thing, player.GridPos, player);
            }
			else
			{
                player.SelectThing(thing);
            }
		}
	}

	public Thing SpawnThingArena(TypeDescription type, IntVector gridPos)
    {
		Host.AssertServer();

		var thing = type.Create<Thing>();
		thing.GridPos = gridPos;

		ArenaGridManager.AddThing(thing);

		return thing;
    }

	public Thing SpawnThingInventory(TypeDescription type, IntVector gridPos, InterfacerPlayer player)
	{
		var thing = type.Create<Thing>();
		thing.Flags |= ThingFlags.InInventory;
		thing.InventoryPlayer = player;
		player.InventoryGridManager.AddThing(thing);
		thing.SetGridPos(gridPos);

        return thing;
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
        Thing otherThing = inventoryGridManager.GetThingAt(targetGridPos);

        inventoryGridManager.DeregisterGridPos(thing, thing.GridPos);
        thing.SetGridPos(targetGridPos);

        if(otherThing != null)
		{
			//Log.Info("otherThing: " + otherThing.DisplayName);
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
            Thing otherThing = inventoryGridManager.GetThingAt(targetGridPos);

            if (otherThing != null)
                MoveThingToArena(otherThing, player.GridPos, player);

            MoveThingToInventory(thing, targetGridPos, player);
        }
        else if (destinationPanelType == PanelType.Nearby)
        {
            player.SelectThing(thing);
        }
    }
}
