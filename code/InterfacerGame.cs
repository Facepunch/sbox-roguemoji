using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Interfacer;

public partial class InterfacerGame : Sandbox.Game
{
	public static InterfacerGame Instance { get; private set; }

	public static int PlayerNum { get; set; }

	public Hud Hud { get; private set; }

	[Net] public GridManager ArenaGridManager { get; private set; }

	public const int ArenaWidth = 30;
	public const int ArenaHeight = 20;
	public const int InventoryWidth = 10;
	public const int InventoryHeight = 5;

	public record struct LogData(string text, int playerNum);
	public Queue<LogData> LogMessageQueue = new Queue<LogData>();

	public InterfacerPlayer LocalPlayer => Local.Client.Pawn as InterfacerPlayer; // Client-only

	public InterfacerGame()
	{
		Instance = this;

		if (Host.IsServer)
		{
			ArenaGridManager = new();
			ArenaGridManager.Init(ArenaWidth, ArenaHeight);

			//Log.Info("------------------ Game() - ArenaGridManager: " + ArenaGridManager);

			SpawnThingArena(TypeLibrary.GetDescription(typeof(Rock)), new IntVector(10, 10));
        }

		if (Host.IsClient)
		{
			Hud = new Hud();
		}
	}

    public override void Spawn()
    {
        base.Spawn();

		//Log.Info("Game:Spawn - ArenaGridManager: " + ArenaGridManager);
	}

    [Event.Tick.Server]
	public void ServerTick()
	{
		//Log.Info("Game:ServerTick - ArenaGridManager: " + ArenaGridManager);

		if (ArenaGridManager == null)
			return;

		float dt = Time.Delta;
		
		ArenaGridManager.Update(dt);
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		//Log.Info("Game:ClientTick - ArenaGridManager: " + ArenaGridManager);

		if (Hud.MainPanel.LogPanel != null)
		{
			while (LogMessageQueue.Count > 0)
			{
				var data = LogMessageQueue.Dequeue();
				Hud.MainPanel.LogPanel.WriteMessage(data.text, data.playerNum);
			}
		}
	}

	public override void ClientJoined(Client client) // Server-only
	{
		base.ClientJoined(client);

		InterfacerPlayer player = SpawnThingArena(TypeLibrary.GetDescription(typeof(InterfacerPlayer)), new IntVector(5, 10)) as InterfacerPlayer;
		player.PlayerNum = ++PlayerNum;
		client.Pawn = player;

		SpawnThingInventory(TypeLibrary.GetDescription(typeof(Rock)), new IntVector(4, 3), player);
	}

	public override void ClientDisconnect(Client client, NetworkDisconnectionReason reason)
	{
		var player = client.Pawn as InterfacerPlayer;
		ArenaGridManager.DeregisterGridPos(player, player.GridPos);
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
	public static void CellClickedArenaCmd(int x, int y, bool rightClick)
	{
		var player = ConsoleSystem.Caller.Pawn as InterfacerPlayer;
		Instance.CellClickedArena(new IntVector(x, y), player, rightClick);
	}

	[ConCmd.Server]
	public static void CellClickedInventoryCmd(int x, int y, bool rightClick)
	{
		var player = ConsoleSystem.Caller.Pawn as InterfacerPlayer;
		Instance.CellClickedInventory(new IntVector(x, y), player, rightClick);
	}

	public void CellClickedArena(IntVector gridPos, InterfacerPlayer player, bool rightClick)
	{
		var thing = ArenaGridManager.GetThingAt(gridPos, ThingFlags.Selectable);

		player.SelectThing(thing);
		LogMessage(player.Client.Name + (rightClick ? " right-clicked " : " clicked ") + (thing != null ? (thing.DisplayIcon + " at ") : "") + gridPos + ".", player.PlayerNum);

		if(thing == null)
        {
			if (Rand.Float(0f, 1f) < 0.1f)
			{
				var rock = Instance.SpawnThingArena(TypeLibrary.GetDescription(typeof(Rock)), gridPos);
				LogMessage(player.Client.Name + " created " + rock.DisplayIcon + " at " + gridPos + "!", player.PlayerNum);
			}
			else
            {
				var explosion = Instance.SpawnThingArena(TypeLibrary.GetDescription(typeof(Explosion)), gridPos);
				explosion.VfxShake(0.15f, 4f);
				explosion.VfxScale(0.15f, Rand.Float(0.6f, 0.8f), Rand.Float(0.3f, 0.4f));
			}
		}
	}

	public void CellClickedInventory(IntVector gridPos, InterfacerPlayer player, bool rightClick)
	{
		var thing = player.InventoryGridManager.GetThingAt(gridPos, ThingFlags.Selectable);

		player.SelectThing(thing);
		LogMessage(player.Client.Name + (rightClick ? " right-clicked " : " clicked ") + (thing != null ? (thing.DisplayIcon + " at ") : "") + gridPos + " in their inventory.", player.PlayerNum);

		if (thing == null)
		{
			if (Rand.Float(0f, 1f) < 0.1f)
			{
				var rock = Instance.SpawnThingInventory(TypeLibrary.GetDescription(typeof(Rock)), gridPos, player);
				LogMessage(player.Client.Name + " created " + rock.DisplayIcon + " at " + gridPos + " in their inventory!", player.PlayerNum);
			}
			else
			{
				var explosion = Instance.SpawnThingInventory(TypeLibrary.GetDescription(typeof(Explosion)), gridPos, player);
				explosion.VfxShake(0.15f, 4f);
				explosion.VfxScale(0.15f, Rand.Float(0.6f, 0.8f), Rand.Float(0.3f, 0.4f));
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
		thing.GridPos = gridPos;
		thing.IsInInventory = true;
		thing.InventoryPlayer = player;
		player.InventoryGridManager.AddThing(thing);

		return thing;
	}
}
