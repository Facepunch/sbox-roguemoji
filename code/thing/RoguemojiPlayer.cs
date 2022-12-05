using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class RoguemojiPlayer : Thing
{
	private TimeSince _inputRepeatTime;
	private const float MOVE_DELAY = 0.4f;

    [Net] public IntVector CameraGridOffset { get; set; }
    public Vector2 CameraPixelOffset { get; set; } // Client-only

    [Net] public GridManager InventoryGridManager { get; private set; }
    [Net] public GridManager EquipmentGridManager { get; private set; }

    [Net] public Thing SelectedThing { get; private set; }

    [Net] public bool IsDead { get; set; }

    public Dictionary<TypeDescription, PlayerStatus> PlayerStatuses = new Dictionary<TypeDescription, PlayerStatus>();

    [Net] public LevelId CurrentLevelId { get; set; }

    public RoguemojiPlayer()
	{
		IconDepth = 5;
		ShouldLogBehaviour = true;
		DisplayName = "Player";
		Tooltip = "";
        PathfindMovementCost = 10f;

        if (Host.IsServer)
        {
			InventoryGridManager = new();
			InventoryGridManager.Init(RoguemojiGame.InventoryWidth, RoguemojiGame.InventoryHeight);
            InventoryGridManager.GridType = GridType.Inventory;

            EquipmentGridManager = new();
            EquipmentGridManager.Init(RoguemojiGame.EquipmentWidth, RoguemojiGame.EquipmentHeight);
            EquipmentGridManager.GridType = GridType.Equipment;

            SetStartingValues();
        }
	}

    void SetStartingValues()
    {
        DisplayIcon = "🙂";
        Flags = ThingFlags.Solid | ThingFlags.Selectable;
        Hp = MaxHp = 10;
        IsDead = false;
        DoneFirstUpdate = false;
        CurrentLevelId = LevelId.None;
        WieldedThing = null;

        InventoryGridManager.Restart();
        EquipmentGridManager.Restart();

        for (int x = 0; x < RoguemojiGame.InventoryWidth - 2; x++)
            for (int y = 0; y < RoguemojiGame.InventoryHeight - 2; y++)
                SpawnRandomInventoryThing(new IntVector(x, y));

        //for (int x = 0; x < 3; x++)
        //    for (int y = 0; y < 2; y++)
        //        SpawnRandomEquipmentThing(new IntVector(x, y));

        RoguemojiGame.Instance.RefreshGridPanelClient(GridType.Inventory);
        RoguemojiGame.Instance.RefreshGridPanelClient(GridType.Equipment);
        RoguemojiGame.Instance.RefreshNearbyPanelClient();
    }

    void SpawnRandomInventoryThing(IntVector gridPos)
    {
        int rand = Rand.Int(0, 8);
        switch (rand)
        {
            case 0: InventoryGridManager.SpawnThing<Leaf>(gridPos); break;
            case 1: InventoryGridManager.SpawnThing<Potato>(gridPos); break;
            case 2: InventoryGridManager.SpawnThing<Nut>(gridPos); break;
            case 3: InventoryGridManager.SpawnThing<Mushroom>(gridPos); break;
            case 4: InventoryGridManager.SpawnThing<Trumpet>(gridPos); break;
            case 5: InventoryGridManager.SpawnThing<Bouquet>(gridPos); break;
            case 6: InventoryGridManager.SpawnThing<Cheese>(gridPos); break;
            case 7: InventoryGridManager.SpawnThing<Coat>(gridPos); break;
            case 8: InventoryGridManager.SpawnThing<SafetyVest>(gridPos); break;
        }
    }

    void SpawnRandomEquipmentThing(IntVector gridPos)
    {
        int rand = Rand.Int(0, 1);
        switch (rand)
        {
            case 0: EquipmentGridManager.SpawnThing<Coat>(gridPos); break;
            case 1: EquipmentGridManager.SpawnThing<SafetyVest>(gridPos); break;
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

            if(Input.Pressed(InputButton.Reload))
            {
                RoguemojiGame.Instance.Restart();
            }

            if (Input.Pressed(InputButton.Slot1))       SelectHotbarSlot(0);
            else if (Input.Pressed(InputButton.Slot2))  SelectHotbarSlot(1);
            else if (Input.Pressed(InputButton.Slot3))  SelectHotbarSlot(2);
            else if (Input.Pressed(InputButton.Slot4))  SelectHotbarSlot(3);
            else if (Input.Pressed(InputButton.Slot5))  SelectHotbarSlot(4);
            else if (Input.Pressed(InputButton.Slot6))  SelectHotbarSlot(5);
            else if (Input.Pressed(InputButton.Slot7))  SelectHotbarSlot(6);
            else if (Input.Pressed(InputButton.Slot8))  SelectHotbarSlot(7);
            else if (Input.Pressed(InputButton.Slot9))  SelectHotbarSlot(8);
            else if (Input.Pressed(InputButton.Slot0))  SelectHotbarSlot(9);

            if(Input.Pressed(InputButton.Use))
                PickUpTopItem();
            else if(Input.Pressed(InputButton.Flashlight))
                DropWieldedItem();
            else if (Input.Pressed(InputButton.Drop))
                DropWieldedItem();
        }
	}

    void SelectHotbarSlot(int index)
    {
        var thing = InventoryGridManager.GetThingsAt(InventoryGridManager.GetGridPos(index)).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing == null)
            return;

        if (Input.Down(InputButton.Run))
        {
            RoguemojiGame.Instance.MoveThingToArena(thing, GridPos, this);
        }
        else
        {
            if(thing.Flags.HasFlag(ThingFlags.Equipment))
            {
                if (EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    RoguemojiGame.Instance.MoveThingToEquipment(thing, emptyGridPos, this);
            }
            else
            {
                WieldThing(thing);
            }
        }
            
    }

	public override bool TryMove( Direction direction, bool shouldAnimate = true )
	{
		var success = base.TryMove( direction, shouldAnimate: false );
		if (success)
		{
			SetIcon("😀");

			var middleCell = new IntVector(MathX.FloorToInt((float)RoguemojiGame.ArenaWidth / 2f), MathX.FloorToInt((float)RoguemojiGame.ArenaHeight / 2f));
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
            {
                VfxSlideCamera(direction, 0.25f, 40f);
                VfxSlide(direction, 0.1f, 40f);
            }
            else
            {
                VfxSlide(direction, 0.2f, 40f);
            }
                
        }
		else 
		{
			SetIcon("🤨");
        }
			
		_inputRepeatTime = 0f;

		return success;
	}

    public override void Interact(Thing other, Direction direction)
    {
        base.Interact(other, direction);

        if(other is Hole)
        {
            if(CurrentLevelId == LevelId.Forest0)
                RoguemojiGame.Instance.SetPlayerLevel(this, LevelId.Forest1);
            else if (CurrentLevelId == LevelId.Forest1)
                RoguemojiGame.Instance.SetPlayerLevel(this, LevelId.Forest2);
        }
        else if(other is Door)
        {
            if (CurrentLevelId == LevelId.Forest1)
                RoguemojiGame.Instance.SetPlayerLevel(this, LevelId.Forest0);
            else if (CurrentLevelId == LevelId.Forest2)
                RoguemojiGame.Instance.SetPlayerLevel(this, LevelId.Forest1);
        }
    }

    public override void SetGridPos(IntVector gridPos)
	{
		base.SetGridPos(gridPos);

        RoguemojiGame.Instance.FlickerNearbyPanelCellsClient();
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
            Math.Clamp(offset.x, 0, ContainingGridManager.GridWidth - RoguemojiGame.ArenaWidth),
            Math.Clamp(offset.y, 0, ContainingGridManager.GridHeight - RoguemojiGame.ArenaHeight)
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
            (gridPos.x < CameraGridOffset.x + RoguemojiGame.ArenaWidth + 1) &&
            (gridPos.y >= CameraGridOffset.y - 1) &&
            (gridPos.y < CameraGridOffset.y + RoguemojiGame.ArenaHeight + 1);
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

    public override void WieldThing(Thing thing)
    {
        if(IsDead)
            return;

        base.WieldThing(thing);
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

    public void PickUpTopItem()
    {
        var thing = ContainingGridManager.GetThingsAt(GridPos).WithNone(ThingFlags.Solid).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing == null)
            return;

        if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
        {
            RoguemojiGame.Instance.MoveThingToInventory(thing, emptyGridPos, this);

            if(WieldedThing == null)
                WieldThing(thing);
        }
        else if(thing.Flags.HasFlag(ThingFlags.Equipment))
        {
            if(EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPosEquipment))
                RoguemojiGame.Instance.MoveThingToEquipment(thing, emptyGridPosEquipment, this);
        }
    }

    public void DropWieldedItem()
    {
        if(WieldedThing != null)
            RoguemojiGame.Instance.MoveThingToArena(WieldedThing, GridPos, this);
    }
}
