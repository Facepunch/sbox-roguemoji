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

	public Hud Hud { get; private set; }
	public GridManager ArenaGridManager { get; private set; }
	public GridManager InventoryGridManager { get; private set; }
	public ThingManager ThingManager { get; private set; }

	public const int ArenaWidth = 30;
	public const int ArenaHeight = 20;
	public const int InventoryWidth = 10;
	public const int InventoryHeight = 5;

	public record struct CellData(GridPanelType gridPanelType, IntVector gridPos, string text, int playerNum, string tooltip, Vector2 offset, float rotationDegrees, float scale);
	public Queue<CellData> WriteCellQueue = new Queue<CellData>();

	public record struct LogData(string text, int playerNum);
	public Queue<LogData> LogMessageQueue = new Queue<LogData>();

	[Net] public int NumThings { get; set; }

	public List<PanelFlickerData> _panelsToFlicker;

	public InterfacerGame()
	{
		Instance = this;

		if (Host.IsServer)
		{
			ThingManager = new ThingManager();
			ArenaGridManager = new GridManager(ArenaWidth, ArenaHeight, GridPanelType.Arena);
			InventoryGridManager = new GridManager(InventoryWidth, InventoryHeight, GridPanelType.Inventory);

			var rock = new Rock()
			{
				GridPos = new IntVector(10, 10),
				GridPanelType = GridPanelType.Arena,
			};
			ThingManager.AddThing(rock);
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

		ThingManager.Update(dt);
		ArenaGridManager.Update();
		InventoryGridManager.Update();

		NumThings = ThingManager.Things.Count;
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		if (Hud.MainPanel.ArenaPanel != null && Hud.MainPanel.InventoryPanel != null)
		{
			while (WriteCellQueue.Count > 0)
			{
				var data = WriteCellQueue.Dequeue();
				RefreshCell(data.gridPanelType, data.gridPos, data.text, data.playerNum, data.tooltip, data.offset, data.rotationDegrees, data.scale);
			}
		}

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

	public override void ClientJoined(Client client)
	{
		base.ClientJoined(client);

		var player = new InterfacerPlayer()
		{
			GridPos = new IntVector(3, 3),
			GridPanelType = GridPanelType.Arena,
		};
		client.Pawn = player;
		ThingManager.AddThing(player);
	}

	public override void ClientDisconnect(Client cl, NetworkDisconnectionReason reason)
	{
		base.ClientDisconnect(cl, reason);

	}

	public void WriteCell(GridPanelType gridPanelType, IntVector gridPos, string text, int playerNum, string tooltip, Vector2 offset, float rotationDegrees, float scale)
	{
		WriteCellClient(gridPanelType, gridPos.x, gridPos.y, text, playerNum, tooltip, offset, rotationDegrees, scale);
	}

	[ClientRpc]
	public void WriteCellClient(GridPanelType gridPanelType, int x, int y, string text, int playerNum, string tooltip, Vector2 offset, float rotationDegrees, float scale)
	{
		var gridPanel = Hud.GetGridPanel(gridPanelType);
		if (gridPanel == null)
		{
			WriteCellQueue.Enqueue(new CellData(gridPanelType, new IntVector(x, y), text, playerNum, tooltip, offset, rotationDegrees, scale));
			return;
		}

		RefreshCell(gridPanelType, new IntVector(x, y), text, playerNum, tooltip, offset, rotationDegrees, scale);
	}

	public void RefreshCell(GridPanelType gridPanelType, IntVector gridPos, string text, int playerNum, string tooltip, Vector2 offset, float rotationDegrees, float scale)
	{
		GridPanel gridPanel = Hud.GetGridPanel(gridPanelType);
		if (gridPanel == null)
		{
			Log.Error("RefreshCell: " + gridPanelType + " - no grid panel of this type!");
			return;
		}

		var cell = gridPanel.GetCell(gridPos);
		if (cell != null)
		{
			cell.SetText(text);
			cell.SetPlayerNum(playerNum);
			cell.SetTooltip(tooltip);
			cell.SetTransform(offset, rotationDegrees, scale);

			cell.Refresh();
		}
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

		var rock = new Rock()
		{
			GridPos = new IntVector(x, y),
			GridPanelType = gridPanelType,
		};
		ThingManager.Instance.AddThing(rock);
	}

	public void FlickerPanel(Panel panel)
	{
		if (panel == null)
			return;

		panel.Style.PointerEvents = PointerEvents.None;
		_panelsToFlicker.Add(new PanelFlickerData(panel));
	}

	public GridManager GetGridManager(GridPanelType gridPanelType)
	{
		if (gridPanelType == GridPanelType.Arena)
			return ArenaGridManager;
		else if (gridPanelType == GridPanelType.Inventory)
			return InventoryGridManager;

		return null;
	}

	[ClientRpc]
	public void VfxNudgeClient(GridPanelType gridPanelType, int x, int y, Direction direction, float lifetime, float distance)
	{
		GridPanel gridPanel = Hud.GetGridPanel(gridPanelType);
		var nudge = gridPanel.AddCellVfx(new IntVector(x, y), TypeLibrary.GetDescription(typeof(VfxNudge))) as VfxNudge;
		nudge.Direction = direction;
		nudge.Lifetime = lifetime;
		nudge.Distance = distance;
	}
}
