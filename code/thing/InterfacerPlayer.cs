using Sandbox;
using System;
using System.Collections.Generic;

namespace Interfacer;
public partial class InterfacerPlayer : Thing
{
	private TimeSince _inputRepeatTime;
	private const float MOVE_DELAY = 0.3f;

    [Net] public IntVector CameraGridOffset { get; set; }

    [Net] public GridManager InventoryGridManager { get; private set; }

	[Net] public Thing SelectedThing { get; private set; }

	public InterfacerPlayer()
	{
		DisplayIcon = "🙂";
		IconDepth = 5;
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

	[Event.Tick.Client]
	public override void ClientTick()
	{
		base.ClientTick();

        //DrawDebugText("" + (SelectedThing == null ? "none" : SelectedThing.Name));
        //DrawDebugText("# Things: " + InventoryGridManager.Things.Count);
        //Log.Info("Player:ClientTick - InventoryGridManager: " + InventoryGridManager);
    }

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

			var middleCell = new IntVector(MathX.FloorToInt((float)InterfacerGame.ArenaWidth / 2f), MathX.FloorToInt((float)InterfacerGame.ArenaHeight / 2f));
            var offsetGridPos = GridPos - CameraGridOffset;

			if(direction == Direction.Left || direction == Direction.Right)
			{
                if (offsetGridPos.x < middleCell.x)
                    SetCameraGridOffset(CameraGridOffset + new IntVector(-1, 0));
                else if (offsetGridPos.x > middleCell.x)
                    SetCameraGridOffset(CameraGridOffset + new IntVector(1, 0));
            }
            
			if(direction == Direction.Down || direction == Direction.Up)
			{
                if (offsetGridPos.y < middleCell.y)
                    SetCameraGridOffset(CameraGridOffset + new IntVector(0, -1));
                else if (offsetGridPos.y > middleCell.y)
                    SetCameraGridOffset(CameraGridOffset + new IntVector(0, 1));
            }
        }
		else 
		{
			SetIcon("🤨");
            VfxNudge(direction, 0.1f, 10f);
		}
			
		_inputRepeatTime = 0f;

		return success;
	}

    public override void SetGridPos(IntVector gridPos, bool forceRefresh = false)
	{
		if (GridPos.Equals(gridPos) && !forceRefresh)
			return;

		base.SetGridPos(gridPos, forceRefresh);

        InterfacerGame.Instance.FlickerNearbyPanelCellsClient();
    }

	public void SelectThing(Thing thing)
	{
		if (SelectedThing == thing)
			return;

		if (SelectedThing != null)
			SelectedThing.RefreshGridPanelClient();

		SelectedThing = thing;
	}

    public void SetCameraGridOffset(IntVector offset)
    {
        CameraGridOffset = new IntVector(
            Math.Clamp(offset.x, 0, ContainingGridManager.LevelWidth - InterfacerGame.ArenaWidth),
            Math.Clamp(offset.y, 0, ContainingGridManager.LevelHeight - InterfacerGame.ArenaHeight)
        );
    }

    public bool IsGridPosVisible(IntVector gridPos)
    {
        return
            gridPos.x >= CameraGridOffset.x &&
            gridPos.x < CameraGridOffset.x + InterfacerGame.ArenaWidth &&
            gridPos.y >= CameraGridOffset.y &&
            gridPos.y < CameraGridOffset.y + InterfacerGame.ArenaHeight;
    }
}
