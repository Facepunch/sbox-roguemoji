﻿using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Interfacer;

public partial class InterfacerGame : Sandbox.Game
{
	public static InterfacerGame Instance { get; private set; }

	public static int PlayerNum { get; set; }

	public Hud Hud { get; private set; }
	public GridManager ArenaGridManager { get; private set; }
	public GridManager InventoryGridManager { get; private set; }
	[Net] public ThingManager ThingManager { get; private set; }

	public const int ArenaWidth = 30;
	public const int ArenaHeight = 20;
	public const int InventoryWidth = 10;
	public const int InventoryHeight = 5;

	public record struct LogData(string text, int playerNum);
	public Queue<LogData> LogMessageQueue = new Queue<LogData>();

	public InterfacerGame()
	{
		Instance = this;

		if (Host.IsServer)
		{
			ThingManager = new ThingManager();
			ArenaGridManager = new GridManager(ArenaWidth, ArenaHeight, GridPanelType.Arena);
			InventoryGridManager = new GridManager(InventoryWidth, InventoryHeight, GridPanelType.Inventory);

			SpawnThing(TypeLibrary.GetDescription(typeof(Rock)), new IntVector(10, 10), GridPanelType.Arena);
			SpawnThing(TypeLibrary.GetDescription(typeof(Rock)), new IntVector(4, 3), GridPanelType.Inventory);
		}

		if (Host.IsClient)
		{
			Hud = new Hud();
		}
	}

	[Event.Tick.Server]
	public void ServerTick()
	{
		float dt = Time.Delta;

		ThingManager.Update(dt);
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
	}

	public override void ClientJoined(Client client)
	{
		base.ClientJoined(client);

		InterfacerPlayer player = SpawnThing(TypeLibrary.GetDescription(typeof(InterfacerPlayer)), new IntVector(5, 10), GridPanelType.Arena) as InterfacerPlayer;
		player.PlayerNum = ++PlayerNum;

		client.Pawn = player;
	}

	public override void ClientDisconnect(Client cl, NetworkDisconnectionReason reason)
	{
		base.ClientDisconnect(cl, reason);

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
	public static void CellClicked(GridPanelType gridPanelType, int x, int y)
	{
		var player = ConsoleSystem.Caller.Pawn as InterfacerPlayer;

		Instance.LogMessage(player.Client.Name + " clicked (" + x + ", " + y + ") in the " + gridPanelType + ".", player.PlayerNum);
		Instance.SpawnThing(TypeLibrary.GetDescription(typeof(Rock)), new IntVector(x, y), gridPanelType);
	}

	public Thing SpawnThing(TypeDescription type, IntVector gridPos, GridPanelType gridPanelType)
    {
		var thing = type.Create<Thing>();
		thing.GridPos = gridPos;
		thing.GridPanelType = gridPanelType;
		ThingManager.AddThing(thing);

		return thing;
    }

	public GridManager GetGridManager(GridPanelType gridPanelType)
	{
		if (gridPanelType == GridPanelType.Arena)
			return ArenaGridManager;
		else if (gridPanelType == GridPanelType.Inventory)
			return InventoryGridManager;

		return null;
	}
}
