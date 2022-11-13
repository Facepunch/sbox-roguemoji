using Sandbox;
using System.Collections.Generic;

namespace Interfacer;

public partial class InterfacerGame : Sandbox.Game
{
	public static InterfacerGame Instance { get; private set; }

	public static int PlayerNum { get; set; }

	public Hud Hud { get; private set; }
	public GridManager GridManager { get; private set; }
	public ThingManager ThingManager { get; private set; }

	public const int GridWidth = 30;
	public const int GridHeight = 20;

	public record struct CellData( IntVector gridPos, string text, int playerNum, string tooltip, Vector2 offset, float rotationDegrees);
	public Queue<CellData> WriteCellQueue = new Queue<CellData> ();

	public record struct LogData( string text, int playerNum );
	public Queue<LogData> LogMessageQueue = new Queue<LogData> ();

	[Net] public int NumThings { get; set; }

	public InterfacerGame()
	{
		Instance = this;

		if(Host.IsServer)
		{
			GridManager = new GridManager( GridWidth, GridHeight );
			ThingManager = new ThingManager();

			var rock = new Rock()
			{
				GridPos = new IntVector(10, 10)
			};
			ThingManager.AddThing( rock );
		}

		if (Host.IsClient)
		{
			Hud = new Hud( GridWidth, GridHeight );
		}
	}

	[Event.Tick.Server]
	public void ServerTick()
	{
		float dt = Time.Delta;

		ThingManager.Update( dt );
		GridManager.Update();

		NumThings = ThingManager.Things.Count;
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		if( Hud.MainPanel.GridPanel != null)
		{
			while ( WriteCellQueue.Count > 0 )
			{
				var data = WriteCellQueue.Dequeue();
				RefreshCell(data.gridPos, data.text, data.playerNum, data.tooltip, data.offset, data.rotationDegrees);
			}
		}

		if( Hud.MainPanel.LogPanel != null)
		{
			while ( LogMessageQueue.Count > 0 )
			{
				var data = LogMessageQueue.Dequeue();
				Hud.MainPanel.LogPanel.WriteMessage( data.text, data.playerNum );
			}
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new InterfacerPlayer()
		{
			GridPos = new IntVector( 3, 3 ),
		};
		client.Pawn = player;
		ThingManager.AddThing( player );
	}

    public override void ClientDisconnect(Client cl, NetworkDisconnectionReason reason)
    {
        base.ClientDisconnect(cl, reason);

    }

    public void WriteCell( IntVector gridPos, string text, int playerNum, string tooltip, Vector2 offset, float rotationDegrees)
	{
		WriteCellClient( gridPos.x, gridPos.y, text, playerNum, tooltip, offset, rotationDegrees);
	}

	[ClientRpc]
	public void WriteCellClient(int x, int y, string text, int playerNum, string tooltip, Vector2 offset, float rotationDegrees)
	{
		if (Hud.MainPanel.GridPanel == null)
		{
			WriteCellQueue.Enqueue(new CellData(new IntVector(x, y), text, playerNum, tooltip, offset, rotationDegrees) );
			return;
		}

		RefreshCell(new IntVector(x, y), text, playerNum, tooltip, offset, rotationDegrees);
	}

	public void RefreshCell(IntVector gridPos, string text, int playerNum, string tooltip, Vector2 offset, float rotationDegrees)
    {
		var cell = Hud.MainPanel.GridPanel.GetCell(gridPos.x, gridPos.y);
		if (cell != null)
		{
			cell.SetText(text);
			cell.SetPlayerNum(playerNum);
			cell.SetTooltip(tooltip);
			cell.SetTransform(offset, rotationDegrees);

			cell.Refresh();
		}
	}

	public void LogMessage( string text, int playerNum )
	{
		LogMessageClient( text, playerNum );
	}

	[ClientRpc]
	public void LogMessageClient( string text, int playerNum )
	{
		if( Hud.MainPanel.LogPanel == null)
		{
			LogMessageQueue.Enqueue( new LogData(text, playerNum) );
			return;
		}

		Hud.MainPanel.LogPanel.WriteMessage( text, playerNum );
	}

	[ConCmd.Server]
	public static void CellClicked(int x, int y)
	{
		var player = ConsoleSystem.Caller.Pawn as InterfacerPlayer;

		Instance.LogMessage( player.Client.Name + " clicked (" + x + ", " + y + ").", player.PlayerNum );

		var rock = new Rock()
		{
			GridPos = new IntVector( x, y ),
		};
		ThingManager.Instance.AddThing( rock );
	}
}
