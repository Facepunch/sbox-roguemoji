using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public interface IQueuedAction
{
    public void Execute();
}

public class MoveThingAction : IQueuedAction
{
    public GridType TargetGridType;

    public void Execute()
    {

    }
}

public partial class RoguemojiPlayer : Thing
{
	public TimeSince TimeSinceInput { get; set; }
	[Net] public float MoveDelay { get; set; }
    [Net] public float InputRechargePercent { get; set; }
    [Net] public bool IsInputReady { get; set; }

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
            InventoryGridManager.OwningPlayer = this;

            EquipmentGridManager = new();
            EquipmentGridManager.Init(RoguemojiGame.EquipmentWidth, RoguemojiGame.EquipmentHeight);
            EquipmentGridManager.GridType = GridType.Equipment;
            EquipmentGridManager.OwningPlayer = this;

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
        MoveDelay = TimeSinceInput = 0.5f;
        IsInputReady = true;

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
            bool wasInputReady = IsInputReady;
            IsInputReady = TimeSinceInput >= MoveDelay;
            InputRechargePercent = Math.Clamp(TimeSinceInput / MoveDelay, 0f, 1f);

            if (IsInputReady && !wasInputReady)
                MovementRecharged();

            if (TimeSinceInput > MoveDelay && !IsDead)
            {
				if (Input.Down(InputButton.Left))               TryMove(Direction.Left);
				else if (Input.Down(InputButton.Right))         TryMove(Direction.Right);
				else if (Input.Down(InputButton.Back))          TryMove(Direction.Down);
				else if (Input.Down(InputButton.Forward))       TryMove(Direction.Up);
            }

            if (Input.Pressed(InputButton.Slot1))           WieldHotbarSlot(0);
            else if (Input.Pressed(InputButton.Slot2))      WieldHotbarSlot(1);
            else if (Input.Pressed(InputButton.Slot3))      WieldHotbarSlot(2);
            else if (Input.Pressed(InputButton.Slot4))      WieldHotbarSlot(3);
            else if (Input.Pressed(InputButton.Slot5))      WieldHotbarSlot(4);
            else if (Input.Pressed(InputButton.Slot6))      WieldHotbarSlot(5);
            else if (Input.Pressed(InputButton.Slot7))      WieldHotbarSlot(6);
            else if (Input.Pressed(InputButton.Slot8))      WieldHotbarSlot(7);
            else if (Input.Pressed(InputButton.Slot9))      WieldHotbarSlot(8);
            else if (Input.Pressed(InputButton.Slot0))      WieldHotbarSlot(9);
            else if (Input.Pressed(InputButton.Use))        PickUpTopItem();
            else if (Input.Pressed(InputButton.Flashlight)) DropWieldedItem();
            else if (Input.Pressed(InputButton.Drop))       DropWieldedItem();

            if (Input.Pressed(InputButton.Reload))
            {
                RoguemojiGame.Instance.Restart();
            }
        }
	}
    
    public override void PerformedAction()
    {
        base.PerformedAction();

        TimeSinceInput = 0f;
        IsInputReady = false;
    }

    public void MovementRecharged()
    {

    }

    void WieldHotbarSlot(int index)
    {
        if (!IsInputReady || IsDead)
            return;

        var thing = InventoryGridManager.GetThingsAt(InventoryGridManager.GetGridPos(index)).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing == null)
            return;

        if (Input.Down(InputButton.Run))
        {
            MoveThingTo(thing, GridType.Arena, GridPos);
        }
        else
        {
            if(thing.Flags.HasFlag(ThingFlags.Equipment))
            {
                if (EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    MoveThingTo(thing, GridType.Equipment, emptyGridPos);
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

        PerformedAction();

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

    public void WieldThing(Thing thing, bool dontRequireMove = false)
    {
        if (IsDead || (WieldedThing == thing) || (!IsInputReady && !dontRequireMove))
            return;

        base.WieldThing(thing);

        if(!dontRequireMove)
            PerformedAction();
    }

    public override void Destroy()
    {
        if (IsDead)
            return;

        IsDead = true;
        SetIcon("😑");
    }

    public void Restart()
    {
        SetStartingValues();
    }

    public void PickUpTopItem()
    {
        if (!IsInputReady || IsDead)
            return;

        var thing = ContainingGridManager.GetThingsAt(GridPos).WithNone(ThingFlags.Solid).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing == null)
            return;

        if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            MoveThingTo(thing, GridType.Inventory, emptyGridPos, wieldIfPossible: true);
        else if(thing.Flags.HasFlag(ThingFlags.Equipment) && EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPosEquipment))
            MoveThingTo(thing, GridType.Equipment, emptyGridPosEquipment);
    }

    public void DropWieldedItem()
    {
        if (!IsInputReady || IsDead)
            return;

        if (WieldedThing != null)
            MoveThingTo(WieldedThing, GridType.Arena, GridPos);
    }

    public void MoveThingTo(Thing thing, GridType targetGridType, IntVector targetGridPos, bool dontRequireMove = false, bool wieldIfPossible = false)
    {
        if (IsDead || (!IsInputReady && !dontRequireMove)) 
            return;

        var sourceGridType = thing.ContainingGridManager.GridType;
        Assert.True(sourceGridType != targetGridType);

        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType: sourceGridType);
        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType: targetGridType);

        if(targetGridType == GridType.Arena || sourceGridType == GridType.Arena)
        {
            RoguemojiGame.Instance.RefreshNearbyPanelClient(To.Single(this));
            RoguemojiGame.Instance.FlickerNearbyPanelCellsClient(To.Single(this));
        }

        thing.ContainingGridManager?.RemoveThing(thing);
        var targetGridManager = GetGridManager(targetGridType);

        Thing targetThing = targetGridType != GridType.Arena ? targetGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault() : null;
        IntVector sourceGridPos = thing.GridPos;

        targetGridManager.AddThing(thing);
        thing.SetGridPos(targetGridPos);

        if (targetThing != null)
        {
            if(sourceGridType == GridType.Equipment && targetGridType == GridType.Inventory && !targetThing.Flags.HasFlag(ThingFlags.Equipment))
            {
                if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    ChangeInventoryPos(targetThing, emptyGridPos);
                else
                    MoveThingTo(targetThing, GridType.Arena, GridPos, dontRequireMove: true);
            }
            else
            {
                MoveThingTo(targetThing, sourceGridType, sourceGridPos, dontRequireMove: true);
            }
        }

        if (targetGridType == GridType.Arena && thing == WieldedThing)
            WieldThing(null, dontRequireMove: true);

        if (targetGridType == GridType.Inventory && wieldIfPossible && WieldedThing == null && !thing.Flags.HasFlag(ThingFlags.Equipment))
            WieldThing(thing, dontRequireMove: true);

        if (!dontRequireMove)
            PerformedAction();
    }

    GridManager GetGridManager(GridType gridType)
    {
        switch(gridType)
        {
            case GridType.Arena:
                return ContainingGridManager;
            case GridType.Inventory:
                return InventoryGridManager;
            case GridType.Equipment:
                return EquipmentGridManager;
        }

        return null;
    }

    //public void MoveThingToArena(Thing thing, IntVector gridPos, bool dontRequireMove = false)
    //{
    //    if (IsDead || (!IsInputReady && !dontRequireMove))
    //        return;

    //    Assert.True(thing.ContainingGridManager != ContainingGridManager);

    //    thing.ContainingGridManager?.RemoveThing(thing);
    //    RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType: GridType.Inventory);
    //    RoguemojiGame.Instance.RefreshNearbyPanelClient(To.Single(this));

    //    ContainingGridManager.AddThing(thing);
    //    thing.SetGridPos(gridPos);

    //    if (WieldedThing == thing)
    //        WieldThing(null);

    //    PerformedAction();
    //    RoguemojiGame.Instance.LogMessage(DisplayIcon + "(" + DisplayName + ") dropped " + thing.DisplayIcon, PlayerNum);
    //}

    //public void MoveThingToInventory(Thing thing, IntVector gridPos, bool dontRequireMove = false, bool wieldIfPossible = false)
    //{
    //    if (IsDead || (!IsInputReady && !dontRequireMove))
    //        return;

    //    Assert.True(thing.ContainingGridManager.GridType != GridType.Inventory);

    //    bool fromNearby = thing.ContainingGridManager.GridType == GridType.Arena;

    //    thing.ContainingGridManager?.RemoveThing(thing);

    //    if (fromNearby)
    //    {
    //        RoguemojiGame.Instance.RefreshNearbyPanelClient(To.Single(this));
    //        RoguemojiGame.Instance.FlickerNearbyPanelCellsClient(To.Single(this));
    //    }

    //    InventoryGridManager.AddThing(thing);
    //    thing.SetGridPos(gridPos);

    //    PerformedAction();
    //    RoguemojiGame.Instance.LogMessage(DisplayIcon + "(" + DisplayName + ") picked up " + thing.DisplayIcon, PlayerNum);

    //    if (wieldIfPossible && WieldedThing == null)
    //        WieldThing(thing, dontRequireMove: true);
    //}

    //public void MoveThingToEquipment(Thing thing, IntVector gridPos, bool dontRequireMove = false)
    //{
    //    if (IsDead || (!IsInputReady && !dontRequireMove))
    //        return;

    //    if (thing.ContainingGridManager.GridType == GridType.Equipment)
    //    {
    //        Log.Error(thing.DisplayName + " at " + gridPos + " is already equipped by " + DisplayName + "!");
    //        return;
    //    }

    //    bool fromNearby = thing.ContainingGridManager.GridType == GridType.Arena;

    //    thing.ContainingGridManager?.RemoveThing(thing);

    //    if (fromNearby)
    //    {
    //        RoguemojiGame.Instance.RefreshNearbyPanelClient(To.Single(this));
    //        RoguemojiGame.Instance.FlickerNearbyPanelCellsClient(To.Single(this));
    //    }

    //    Thing targetThing = EquipmentGridManager.GetThingsAt(gridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
    //    IntVector originalGridPos = thing.GridPos;


    //    EquipmentGridManager.AddThing(thing);
    //    thing.SetGridPos(gridPos);

    //    PerformedAction();
    //    RoguemojiGame.Instance.LogMessage(DisplayIcon + "(" + DisplayName + ") equipped " + thing.DisplayIcon, PlayerNum);

    //    if (targetThing != null)
    //        MoveThingToInventory(targetThing, originalGridPos, dontRequireMove: true);
    //}

    public void ChangeInventoryPos(Thing thing, IntVector targetGridPos)
    {
        if (IsDead)
            return;

        IntVector currGridPos = thing.GridPos;
        Thing otherThing = InventoryGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        InventoryGridManager.DeregisterGridPos(thing, thing.GridPos);
        thing.SetGridPos(targetGridPos);

        if (otherThing != null)
        {
            InventoryGridManager.DeregisterGridPos(otherThing, otherThing.GridPos);
            otherThing.SetGridPos(currGridPos);
        }

        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType: GridType.Inventory);
    }

    public void ChangeEquipmentPos(Thing thing, IntVector targetGridPos)
    {
        if (IsDead)
            return;

        IntVector currGridPos = thing.GridPos;
        Thing otherThing = EquipmentGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        EquipmentGridManager.DeregisterGridPos(thing, thing.GridPos);
        thing.SetGridPos(targetGridPos);

        if (otherThing != null)
        {
            EquipmentGridManager.DeregisterGridPos(otherThing, otherThing.GridPos);
            otherThing.SetGridPos(currGridPos);
        }

        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType: GridType.Equipment);
    }

    public void GridCellClicked(IntVector gridPos, GridType gridType, bool rightClick, bool shift, bool doubleClick)
    {
        if (gridType == GridType.Arena)
        {
            var level = RoguemojiGame.Instance.Levels[CurrentLevelId];
            var thing = level.GridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (!rightClick)
                SelectThing(thing);
        }
        else if (gridType == GridType.Inventory)
        {
            var thing = InventoryGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (thing != null && doubleClick && !thing.Flags.HasFlag(ThingFlags.Equipment))
            {
                WieldThing(thing);
            }
            else if (!rightClick)
            {
                if (thing != null && shift)
                    MoveThingTo(thing, GridType.Arena, GridPos);
                else
                    SelectThing(thing);
            }
            else
            {
                if (thing != null && thing.Flags.HasFlag(ThingFlags.Equipment))
                {
                    if (EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                        MoveThingTo(thing, GridType.Equipment, emptyGridPos);
                }
                else
                {
                    WieldThing(thing);
                }
            }
        }
        else if (gridType == GridType.Equipment)
        {
            var thing = EquipmentGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (!rightClick)
            {
                if (thing != null && shift)
                    MoveThingTo(thing, GridType.Arena, GridPos);
                else
                    SelectThing(thing);
            }
            else
            {
                if (thing != null && InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    MoveThingTo(thing, GridType.Inventory, emptyGridPos);
            }
        }
    }

    public void NearbyThingClicked(Thing thing, bool rightClick, bool shift, bool doubleClick)
    {
        if (shift || rightClick || doubleClick)
        {
            if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                MoveThingTo(thing, GridType.Inventory, emptyGridPos, wieldIfPossible: true);
        }
        else
        {
            SelectThing(thing);
        }
    }

    public void InventoryThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos)
    {
        if (destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.Nearby || destinationPanelType == PanelType.None)
        {
            MoveThingTo(thing, GridType.Arena, GridPos);
        }
        else if (destinationPanelType == PanelType.InventoryGrid)
        {
            if (!thing.GridPos.Equals(targetGridPos))
                ChangeInventoryPos(thing, targetGridPos);
            else
                SelectThing(thing);
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.Flags.HasFlag(ThingFlags.Equipment))
                return;

            MoveThingTo(thing, GridType.Equipment, targetGridPos);
        }
        else if (destinationPanelType == PanelType.Wielding)
        {
            if (WieldedThing == thing)
                SelectThing(thing);
            else if (!thing.Flags.HasFlag(ThingFlags.Equipment))
                WieldThing(thing);
        }
        else if (destinationPanelType == PanelType.PlayerIcon)
        {
            if (thing.Flags.HasFlag(ThingFlags.Equipment))
            {
                if (EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    MoveThingTo(thing, GridType.Equipment, emptyGridPos);
            }
            else
            {
                WieldThing(thing);
            }
        }
    }

    public void EquipmentThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos)
    {
        if (destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.Nearby || destinationPanelType == PanelType.None)
        {
            MoveThingTo(thing, GridType.Arena, GridPos);
        }
        else if (destinationPanelType == PanelType.InventoryGrid)
        {
            MoveThingTo(thing, GridType.Inventory, targetGridPos);
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.GridPos.Equals(targetGridPos))
                ChangeEquipmentPos(thing, targetGridPos);
            else
                SelectThing(thing);
        }
    }

    public void NearbyThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos)
    {
        // dont allow dragging nearby thing from different cells, or if the thing has been picked up by someone else
        if (!GridPos.Equals(thing.GridPos) || thing.ContainingGridManager.GridType == GridType.Inventory)
            return;

        if (destinationPanelType == PanelType.InventoryGrid)
        {
            MoveThingTo(thing, GridType.Inventory, targetGridPos);
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.Flags.HasFlag(ThingFlags.Equipment))
                return;

            MoveThingTo(thing, GridType.Equipment, targetGridPos);
        }
        else if (destinationPanelType == PanelType.Nearby)
        {
            SelectThing(thing);
        }
        else if (destinationPanelType == PanelType.Wielding)
        {
            if (thing.Flags.HasFlag(ThingFlags.Equipment))
                return;

            if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            {
                MoveThingTo(thing, GridType.Inventory, emptyGridPos, wieldIfPossible: true);
            }
        }
        else if (destinationPanelType == PanelType.PlayerIcon)
        {
            // todo
        }
    }

    public void WieldingClicked(bool rightClick, bool shift)
    {
        if (WieldedThing == null)
            return;

        if (rightClick)
            WieldThing(null);
        else if (shift)
            MoveThingTo(WieldedThing, GridType.Arena, GridPos);
        else
            SelectThing(WieldedThing);
    }

    public void PlayerIconClicked(bool rightClick, bool shift)
    {
        SelectThing(this);
    }
}
