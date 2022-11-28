using Sandbox;
using System;
using System.Collections.Generic;

namespace Interfacer;
public partial class InterfacerPlayer : Thing
{
	private TimeSince _inputRepeatTime;
	private const float MOVE_DELAY = 0.3f;

    [Net] public IntVector CameraGridOffset { get; set; }
    public Vector2 CameraPixelOffset { get; set; }

    [Net] public GridManager InventoryGridManager { get; private set; }

	[Net] public Thing SelectedThing { get; private set; }

    public Dictionary<TypeDescription, PlayerStatus> PlayerStatuses = new Dictionary<TypeDescription, PlayerStatus>();

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

        float dt = Time.Delta;
        foreach (KeyValuePair<TypeDescription, PlayerStatus> pair in PlayerStatuses)
        {
            var status = pair.Value;
            if (status.ShouldUpdate)
                status.Update(dt);
        }

        //DrawDebugText("" + CameraGridOffset + ", " + CameraPixelOffset);
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

        //var t = Time.Now * 0.4f;
        //CameraPixelOffset = new Vector2(MathF.Sin(t) * 40f, MathF.Sin(t) * 40f);

        foreach (KeyValuePair<TypeDescription, PlayerStatus> pair in PlayerStatuses)
        {
            var status = pair.Value;
            if (status.ShouldUpdate)
                status.Update(dt);
        }
    }

	public override void Simulate( Client cl )
	{
		if(Host.IsServer)
		{
			if (_inputRepeatTime > MOVE_DELAY)
            {
				if (Input.Down(InputButton.Left))           TryMove(Direction.Left);
				else if (Input.Down(InputButton.Right))     TryMove(Direction.Right);
				else if (Input.Down(InputButton.Back))      TryMove(Direction.Down);
				else if (Input.Down(InputButton.Forward))   TryMove(Direction.Up);
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
            var movedCamera = false;

			if(direction == Direction.Left || direction == Direction.Right)
			{
                if (offsetGridPos.x < middleCell.x)         movedCamera = SetCameraGridOffset(CameraGridOffset + new IntVector(-1, 0));
                else if (offsetGridPos.x > middleCell.x)    movedCamera = SetCameraGridOffset(CameraGridOffset + new IntVector(1, 0));
            }
            
			if(direction == Direction.Down || direction == Direction.Up)
			{
                if (offsetGridPos.y < middleCell.y)         movedCamera = SetCameraGridOffset(CameraGridOffset + new IntVector(0, -1));
                else if (offsetGridPos.y > middleCell.y)    movedCamera = SetCameraGridOffset(CameraGridOffset + new IntVector(0, 1));
            }

            if(movedCamera)
                VfxSlideCamera(direction, 0.25f, 40f);
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

    /// <summary>Returns true if offset changed.</summary>
    public bool SetCameraGridOffset(IntVector offset)
    {
        var currOffset = CameraGridOffset;

        CameraGridOffset = new IntVector(
            Math.Clamp(offset.x, 0, ContainingGridManager.LevelWidth - InterfacerGame.ArenaWidth),
            Math.Clamp(offset.y, 0, ContainingGridManager.LevelHeight - InterfacerGame.ArenaHeight)
        );

        return !CameraGridOffset.Equals(currOffset);
    }

    public void SetCameraPixelOffset(Vector2 offset)
    {
        CameraPixelOffset = offset;
    }

    public bool IsGridPosVisible(IntVector gridPos)
    {
        return
            gridPos.x >= CameraGridOffset.x &&
            gridPos.x < CameraGridOffset.x + InterfacerGame.ArenaWidth &&
            gridPos.y >= CameraGridOffset.y &&
            gridPos.y < CameraGridOffset.y + InterfacerGame.ArenaHeight;
    }

    public PlayerStatus AddPlayerStatus(TypeDescription type)
    {
        if (PlayerStatuses.ContainsKey(type))
        {
            var status = PlayerStatuses[type];
            status.ReInitialize();
            return status;
        }
        else
        {
            var status = type.Create<PlayerStatus>();
            status.Init(this);
            PlayerStatuses.Add(type, status);
            return status;
        }
    }

    public void RemovePlayerStatus(TypeDescription type)
    {
        if (PlayerStatuses.ContainsKey(type))
        {
            var status = PlayerStatuses[type];
            status.OnRemove();
            PlayerStatuses.Remove(type);
        }
    }

    public void ForEachPlayerStatus(Action<PlayerStatus> action)
    {
        foreach (var (_, status) in PlayerStatuses)
        {
            action(status);
        }
    }

    [ClientRpc]
    public void VfxSlideCamera(Direction direction, float lifetime, float distance)
    {
        var slide = AddPlayerStatus(TypeLibrary.GetDescription(typeof(VfxPlayerSlideCameraStatus))) as VfxPlayerSlideCameraStatus;
        slide.Direction = direction;
        slide.Lifetime = lifetime;
        slide.Distance = distance;
    }
}
