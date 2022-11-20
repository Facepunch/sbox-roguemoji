using Sandbox;
using System;
using System.Collections.Generic;

namespace Interfacer;
public partial class InterfacerPlayer : Thing
{
	private TimeSince _inputRepeatTime;
	private const float MOVE_DELAY = 0.3f;

	[Net] public GridManager InventoryGridManager { get; private set; }

	[Net] public Thing SelectedThing { get; private set; }

	public InterfacerPlayer()
	{
		DisplayIcon = "🙂";
		IconDepth = 2;
		ShouldLogBehaviour = true;
		DisplayName = "Player";
		Tooltip = "";
		Flags = ThingFlags.Solid | ThingFlags.Selectable;

		if(Host.IsServer)
        {
			InventoryGridManager = new();
			InventoryGridManager.Init(InterfacerGame.InventoryWidth, InterfacerGame.InventoryHeight);
		}
	}

	public override void Spawn()
	{
		base.Spawn();

	}

    public override void OnClientActive(Client client)
    {
        base.OnClientActive(client);

		//Log.Info("OnClientActive - client: " + client);

		DisplayName = Client.Name;
		Tooltip = Client.Name;
	}

	//[Event.Tick.Client]
	//public override void ClientTick()
 //   {
	//	base.ClientTick();

	//	//Log.Info("Player:ClientTick - InventoryGridManager: " + InventoryGridManager);
	//}

	//[Event.Tick.Server]
	//public void ServerTick()
	//{
	//	//Log.Info("Player:ServerTick - InventoryGridManager: " + InventoryGridManager);
	//}

	public override void Update( float dt )
	{
		base.Update( dt );

		InventoryGridManager.Update(dt);
	}

	public override void Simulate( Client cl )
	{
		if(Host.IsServer)
		{
			if (_inputRepeatTime > MOVE_DELAY)
            {
				if (Input.Down(InputButton.Left))
					TryMove(Direction.Left);
				else if (Input.Down(InputButton.Right))
					TryMove(Direction.Right);
				else if (Input.Down(InputButton.Back))
					TryMove(Direction.Down);
				else if (Input.Down(InputButton.Forward))
					TryMove(Direction.Up);
			}
		}
	}

	public override bool TryMove( Direction direction )
	{
		var success = base.TryMove( direction );
		if (success)
		{
			SetIcon("😀");
		}
		else 
		{
			SetIcon("🤨");
            VfxNudge(direction, 0.1f, 10f);
		}
			
		_inputRepeatTime = 0f;

		return success;
	}

	void UpdateThings(IList<Thing> things, float dt)
	{
		for (int i = things.Count - 1; i >= 0; i--)
		{
			var thing = things[i];

			if (!thing.DoneFirstUpdate)
				thing.FirstUpdate();

			if (thing.ShouldUpdate || thing.Statuses.Count > 0)
				thing.Update(dt);
		}
	}

	public void SelectThing(Thing thing)
	{
		if (SelectedThing == thing)
			return;

		SelectedThing = thing;
	}
}
