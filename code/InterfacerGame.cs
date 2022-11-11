using Sandbox;
using System.Collections.Generic;

namespace Interfacer;

public class WriteCellData
{
	public IntVector gridPos;
	public string text;
}

public partial class InterfacerGame : Sandbox.Game
{
	public static InterfacerGame Instance { get; private set; }

	public static int PlayerNum { get; set; }

	public Hud Hud { get; private set; }
	public GridManager GridManager { get; private set; }
	public ThingManager ThingManager { get; private set; }

	public const int GridWidth = 35;
	public const int GridHeight = 20;

	public record struct CellData( IntVector gridPos, string text, int playerNum);
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
				var cell = Hud.MainPanel.GridPanel.GetCell( data.gridPos );
				if ( cell != null )
				{
					cell.SetText( data.text );
					cell.SetPlayerNum( data.playerNum);
					cell.Refresh();
				}
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

	public void WriteCell( IntVector gridPos, string text, int playerNum)
	{
		WriteCellClient( gridPos.x, gridPos.y, text, playerNum);
	}

	[ClientRpc]
	public void WriteCellClient(int x, int y, string text, int playerNum)
	{
		if (Hud.MainPanel.GridPanel == null)
		{
			WriteCellQueue.Enqueue(new CellData(new IntVector(x, y), text, playerNum ) );
			return;
		}

		var cell = Hud.MainPanel.GridPanel.GetCell( x, y );

		if ( cell != null )
		{
			cell.SetText( text );
			cell.SetPlayerNum( playerNum );
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
