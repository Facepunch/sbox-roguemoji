using Sandbox;
using System;
using System.Collections.Generic;

namespace Interfacer;
public partial class InterfacerPlayer : Thing
{
	private TimeSince _inputRepeatTime;
	private const float MOVE_DELAY = 0.4f;

    [Net] public IntVector CameraGridOffset { get; set; }
    public Vector2 CameraPixelOffset { get; set; }

    [Net] public GridManager InventoryGridManager { get; private set; }

	[Net] public Thing SelectedThing { get; private set; }

    [Net] public bool IsDead { get; set; }

    public Dictionary<TypeDescription, PlayerStatus> PlayerStatuses = new Dictionary<TypeDescription, PlayerStatus>();

    public InterfacerPlayer()
	{
		DisplayIcon = "🙂";
		IconDepth = 5;
		ShouldLogBehaviour = true;
		DisplayName = "Player";
		Tooltip = "";
        PathfindMovementCost = 10f;

        if (Host.IsServer)
        {
			InventoryGridManager = new();
			InventoryGridManager.Init(InterfacerGame.InventoryWidth, InterfacerGame.InventoryHeight);

            SetStartingValues();
        }
	}

    void SetStartingValues()
    {
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        Hp = MaxHp = 10;
        IsDead = false;
        DoneFirstUpdate = false;

        InventoryGridManager.Restart();

        for (int x = 0; x < InterfacerGame.InventoryWidth; x++)
        {
            for (int y = 0; y < InterfacerGame.InventoryHeight; y++)
            {
                InterfacerGame.Instance.SpawnThingInventory(TypeLibrary.GetDescription(GetRandomType()), new IntVector(x, y), this);
            }
        }
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

    public override void OnClientActive(Client client)
    {
        base.OnClientActive(client);

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

    public override void Update( float dt )
	{
		base.Update( dt );

		InventoryGridManager.Update(dt);

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
			if (_inputRepeatTime > MOVE_DELAY && !IsDead)
            {
				if (Input.Down(InputButton.Left))           TryMove(Direction.Left);
				else if (Input.Down(InputButton.Right))     TryMove(Direction.Right);
				else if (Input.Down(InputButton.Back))      TryMove(Direction.Down);
				else if (Input.Down(InputButton.Forward))   TryMove(Direction.Up);
			}

            if(Input.Pressed(InputButton.Run))
            {
                InterfacerGame.Instance.Restart();
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
        CameraPixelOffset = new Vector2(MathF.Round(offset.x), MathF.Round(offset.y));
    }

    public bool IsGridPosVisible(IntVector gridPos)
    {
        return
            (gridPos.x >= CameraGridOffset.x - 1) &&
            (gridPos.x < CameraGridOffset.x + InterfacerGame.ArenaWidth + 1) &&
            (gridPos.y >= CameraGridOffset.y - 1) &&
            (gridPos.y < CameraGridOffset.y + InterfacerGame.ArenaHeight + 1);
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

    [ClientRpc]
    public void VfxShakeCamera(float lifetime, float distance)
    {
        var shake = AddPlayerStatus(TypeLibrary.GetDescription(typeof(VfxPlayerShakeCameraStatus))) as VfxPlayerShakeCameraStatus;
        shake.Lifetime = lifetime;
        shake.Distance = distance;
    }

    public override void Damage(int amount, Thing source)
    {
        if (IsDead)
            return;

        base.Damage(amount, source);
    }

    public override void Destroy()
    {
        if (IsDead)
            return;

        IsDead = true;
        DisplayIcon = "😑";
    }

    public void Restart()
    {
        SetStartingValues();
    }
}
