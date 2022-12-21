using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum AimingSource { Throwing, UsingWieldedItem }
public enum AimingType { Direction, TargetCell }

public partial class RoguemojiPlayer : Thing
{
	//public TimeSince TimeSinceAction { get; set; }
	//[Net] public float ActionDelay { get; set; }
 //   [Net] public float ActionRechargePercent { get; set; }
 //   [Net] public bool IsActionReady { get; set; }

    public Acting Acting { get; private set; }
    private float _startingActionDelay = 0.5f;

    [Net] public IntVector CameraGridOffset { get; set; }
    public Vector2 CameraPixelOffset { get; set; } // Client-only

    [Net] public GridManager InventoryGridManager { get; private set; }
    [Net] public GridManager EquipmentGridManager { get; private set; }

    [Net] public Thing SelectedThing { get; private set; }

    [Net] public bool IsDead { get; set; }

    public Dictionary<TypeDescription, PlayerComponent> PlayerComponents = new Dictionary<TypeDescription, PlayerComponent>();

    public IQueuedAction QueuedAction { get; private set; }
    [Net] public string QueuedActionName { get; private set; }

    public HashSet<IntVector> VisibleCells { get; set; } // Client-only

    [Net] public bool IsAiming { get; set; }
    [Net] public AimingSource AimingSource { get; set; }
    [Net] public AimingType AimingType { get; set; }
    public HashSet<IntVector> AimingCells { get; set; } // Client-only

    public RoguemojiPlayer()
	{
		IconDepth = 5;
		ShouldLogBehaviour = true;
		DisplayName = "Player";
		Tooltip = "";
        PathfindMovementCost = 10f;

        if (Game.IsServer)
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
        else
        {
            VisibleCells = new HashSet<IntVector>();
            AimingCells = new HashSet<IntVector>();
        }
	}

    public override void Spawn()
    {
        base.Spawn();

        Acting = AddThingComponent<Acting>();
        Acting.ActionDelay = _startingActionDelay;
    }

    void SetStartingValues()
    {
        DisplayIcon = "🙂";
        Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanUseThings;
        IsDead = false;
        DoneFirstUpdate = false;
        CurrentLevelId = LevelId.None;
        //ActionDelay = TimeSinceAction = 0.5f;
        //IsActionReady = true;
        QueuedAction = null;
        QueuedActionName = "";
        RefreshVisibility();
        SightBlockAmount = 10;
        IsAiming = false;

        ClearStats();
        InitStat(StatType.Health, 10, 0, 10);
        InitStat(StatType.Attack, 1);
        InitStat(StatType.Speed, 1);
        InitStat(StatType.Intelligence, 1);
        InitStat(StatType.Charisma, 1);
        InitStat(StatType.Sight, 9);
        InitStat(StatType.Hearing, 3);
        //InitStat(StatType.Smell, 1);

        ClearTraits();
        //AddTrait("Trait", "🌟", "Trait description.", DisplayName);
        //AddTrait("Trait", "🕹️", "Trait description.");
        //AddTrait("Trait", "📮", "Trait description.");
        //AddTrait("Trait", "🌟", "Trait description.");
        //AddTrait("Trait", "🕹️", "Trait description.");
        //AddTrait("Trait", "📮", "Trait description.");

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

    public override void Restart()
    {
        base.Restart();

        SetStartingValues();
        Acting.ActionDelay = _startingActionDelay;
        Acting.IsActionReady = false;
    }

    void SpawnRandomInventoryThing(IntVector gridPos)
    {
        int rand = Game.Random.Int(0, 12);
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
            case 9: InventoryGridManager.SpawnThing<Sunglasses>(gridPos); break;
            case 10: InventoryGridManager.SpawnThing<Telescope>(gridPos); break;
            case 11: InventoryGridManager.SpawnThing<WhiteCane>(gridPos); break;
            case 12: InventoryGridManager.SpawnThing<Scroll>(gridPos); break;
        }
    }

    void SpawnRandomEquipmentThing(IntVector gridPos)
    {
        int rand = Game.Random.Int(0, 1);
        switch (rand)
        {
            case 0: EquipmentGridManager.SpawnThing<Coat>(gridPos); break;
            case 1: EquipmentGridManager.SpawnThing<SafetyVest>(gridPos); break;
        }
    }

    public override void OnClientActive(IClient client)
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
        foreach (KeyValuePair<TypeDescription, PlayerComponent> pair in PlayerComponents)
        {
            var component = pair.Value;
            if (component.ShouldUpdate)
                component.Update(dt);
        }

        //DrawDebugText("" + CameraGridOffset + ", " + CameraPixelOffset);
        //DrawDebugText("# Things: " + InventoryGridManager.Things.Count);
        //Log.Info("Player:Client - Sight: " + GetStat(StatType.Sight));
    }

    public override void Update( float dt )
	{
		base.Update( dt );

        InventoryGridManager.Update(dt);

        foreach (KeyValuePair<TypeDescription, PlayerComponent> pair in PlayerComponents)
        {
            var component = pair.Value;
            if (component.ShouldUpdate)
                component.Update(dt);
        }

        //DebugText = "";
        //if (QueuedAction != null)
        //    DebugText = QueuedActionName;

        //DebugText = $"IsAiming: {IsAiming}";
    }

	public override void Simulate(IClient cl )
	{
		if(Game.IsServer)
		{
            //bool wasInputReady = IsActionReady;
            //IsActionReady = TimeSinceAction >= ActionDelay;
            //ActionRechargePercent = Math.Clamp(TimeSinceAction / ActionDelay, 0f, 1f);

            //if (IsActionReady && !wasInputReady)
            //    ActionRecharged();

            if (!IsDead)
            {
                if (!IsAiming)
                {
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
                    else if (Input.Pressed(InputButton.Drop))       DropWieldedItem();
                    else if (Input.Pressed(InputButton.Jump))       UseWieldedThing();
                    else if (Input.Pressed(InputButton.View))       CharacterHotkeyPressed();
                    else if (Input.Pressed(InputButton.Left))       TryMove(Direction.Left, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Right))      TryMove(Direction.Right, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Back))       TryMove(Direction.Down, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Forward))    TryMove(Direction.Up, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Flashlight)) StartAimingWieldedItem(1);
                    else if (Input.Down(InputButton.Left))          TryMove(Direction.Left);
                    else if (Input.Down(InputButton.Right))         TryMove(Direction.Right);
                    else if (Input.Down(InputButton.Back))          TryMove(Direction.Down);
                    else if (Input.Down(InputButton.Forward))       TryMove(Direction.Up);
                }
                else
                {
                    if (Input.Pressed(InputButton.Left))            ConfirmAiming(Direction.Left);
                    else if (Input.Pressed(InputButton.Right))      ConfirmAiming(Direction.Right);
                    else if (Input.Pressed(InputButton.Back))       ConfirmAiming(Direction.Down);
                    else if (Input.Pressed(InputButton.Forward))    ConfirmAiming(Direction.Up);
                    else if (Input.Pressed(InputButton.Flashlight)) StopAiming();
                }
            }

            if (Input.Pressed(InputButton.Reload))
            {
                RoguemojiGame.Instance.Restart();
            }
        }
	}
    
    //public override void PerformedAction()
    //{
    //    base.PerformedAction();

    //    TimeSinceAction = 0f;
    //    IsActionReady = false;
    //}

    public override void OnActionRecharged()
    {
        if(QueuedAction != null)
        {
            QueuedAction.Execute(this);
            QueuedAction = null;
            QueuedActionName = "";
        }
    }

    void WieldHotbarSlot(int index)
    {
        var thing = InventoryGridManager.GetThingsAt(InventoryGridManager.GetGridPos(index)).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing != null && Input.Down(InputButton.Run))
        {
            MoveThingTo(thing, GridType.Arena, GridPos);
        }
        else
        {
            if(thing != null && thing.Flags.HasFlag(ThingFlags.Equipment))
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

	public new bool TryMove( Direction direction, bool shouldQueueAction = false )
	{
        if (!Acting.IsActionReady)
        {
            if(shouldQueueAction)
            {
                QueuedAction = new TryMoveAction(direction);
                QueuedActionName = QueuedAction.ToString();
            }
            
            return false;
        }

        var success = base.TryMove( direction, shouldAnimate: false );
		if (success)
		{
            if(HasEquipmentType(TypeLibrary.GetType(typeof(Sunglasses))))
                SetIcon("😎");
            else
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
                // todo: make an option to turn this off
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

        Acting.PerformedAction();
        RefreshVisibility();
		return success;
	}

    public override void BumpInto(Thing other, Direction direction)
    {
        base.BumpInto(other, direction);

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

    public bool IsGridPosOnCamera(IntVector gridPos)
    {
        return
            (gridPos.x >= CameraGridOffset.x - 1) &&
            (gridPos.x < CameraGridOffset.x + RoguemojiGame.ArenaWidth + 1) &&
            (gridPos.y >= CameraGridOffset.y - 1) &&
            (gridPos.y < CameraGridOffset.y + RoguemojiGame.ArenaHeight + 1);
    }

    public PlayerComponent AddPlayerComponent(TypeDescription type)
    {
        if (PlayerComponents.ContainsKey(type))
        {
            var component = PlayerComponents[type];
            component.ReInitialize();
            return component;
        }
        else
        {
            var component = type.Create<PlayerComponent>();
            component.Init(this);
            PlayerComponents.Add(type, component);
            return component;
        }
    }

    public void RemovePlayerComponent(TypeDescription type)
    {
        if (PlayerComponents.ContainsKey(type))
        {
            var component = PlayerComponents[type];
            component.OnRemove();
            PlayerComponents.Remove(type);
        }
    }

    public void ForEachPlayerComponent(Action<PlayerComponent> action)
    {
        foreach (var (_, component) in PlayerComponents)
        {
            action(component);
        }
    }

    [ClientRpc]
    public void VfxSlideCamera(Direction direction, float lifetime, float distance)
    {
        var slide = AddPlayerComponent(TypeLibrary.GetType(typeof(VfxPlayerSlideCamera))) as VfxPlayerSlideCamera;
        slide.Direction = direction;
        slide.Lifetime = lifetime;
        slide.Distance = distance;
    }

    [ClientRpc]
    public void VfxShakeCamera(float lifetime, float distance)
    {
        var shake = AddPlayerComponent(TypeLibrary.GetType(typeof(VfxPlayerShakeCamera))) as VfxPlayerShakeCamera;
        shake.Lifetime = lifetime;
        shake.Distance = distance;
    }

    public override void TakeDamage(Thing source)
    {
        if (IsDead)
            return;

        base.TakeDamage(source);
    }

    public override void Destroy()
    {
        if (IsDead)
            return;

        IsDead = true;
        SetIcon("😑");
    }

    public void PickUpTopItem()
    {
        var thing = ContainingGridManager.GetThingsAt(GridPos).WithNone(ThingFlags.Solid).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing == null)
            return;

        if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            MoveThingTo(thing, GridType.Inventory, emptyGridPos, wieldIfPossible: true);
        else if(thing.Flags.HasFlag(ThingFlags.Equipment) && EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPosEquipment))
            MoveThingTo(thing, GridType.Equipment, emptyGridPosEquipment);
    }

    public void ThrowWieldedThing(Direction direction)
    {
        if (!Acting.IsActionReady)
        {
            QueuedAction = new ThrowThingAction(WieldedThing, direction);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        if (WieldedThing == null || direction == Direction.None)
            return;

        var projectile = WieldedThing.AddThingComponent<Projectile>();
        projectile.Direction = direction;
        projectile.MoveDelay = 0.1f;
        projectile.RemainingDistance = 5;

        MoveThingTo(WieldedThing, GridType.Arena, GridPos);
    }

    public void DropWieldedItem()
    {
        if (WieldedThing != null)
            MoveThingTo(WieldedThing, GridType.Arena, GridPos);
    }

    public void MoveThingTo(Thing thing, GridType targetGridType, IntVector targetGridPos, bool dontRequireAction = false, bool wieldIfPossible = false)
    {
        if (IsDead) 
            return;

        if (IsAiming)
            StopAiming();

        if(!Acting.IsActionReady && !dontRequireAction)
        {
            QueuedAction = new MoveThingAction(thing, targetGridType, targetGridPos, thing.ContainingGridManager.GridType, thing.GridPos, wieldIfPossible);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        var sourceGridType = thing.ContainingGridManager.GridType;
        Sandbox.Diagnostics.Assert.True(sourceGridType != targetGridType);

        if(sourceGridType == GridType.Equipment)
            thing.ContainingGridManager.OwningPlayer.UnequipThing(thing);

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
                    SwapGridThingPos(targetThing, GridType.Inventory, emptyGridPos);
                else
                    MoveThingTo(targetThing, GridType.Arena, GridPos, dontRequireAction: true);
            }
            else
            {
                MoveThingTo(targetThing, sourceGridType, sourceGridPos, dontRequireAction: true);
            }
        }

        if (targetGridType == GridType.Arena && thing == WieldedThing)
            WieldThing(null, dontRequireAction: true);

        if (targetGridType == GridType.Inventory && wieldIfPossible && WieldedThing == null && !thing.Flags.HasFlag(ThingFlags.Equipment))
            WieldThing(thing, dontRequireAction: true);

        if (targetGridType == GridType.Equipment)
            targetGridManager.OwningPlayer.EquipThing(thing);

        if (!dontRequireAction)
            Acting.PerformedAction();
    }

    public void WieldThing(Thing thing, bool dontRequireAction = false)
    {
        if (IsDead || WieldedThing == thing)
            return;

        if (IsAiming)
            StopAiming();

        if (!Acting.IsActionReady && !dontRequireAction)
        {
            QueuedAction = new WieldThingAction(thing);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        base.WieldThing(thing);

        RoguemojiGame.Instance.FlickerWieldingPanel();

        if (!dontRequireAction)
            Acting.PerformedAction();
    }

    public void SwapGridThingPos(Thing thing, GridType gridType, IntVector targetGridPos)
    {
        if (IsDead || gridType == GridType.Arena)
            return;

        var gridManager = GetGridManager(gridType);
        Thing targetThing = gridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        IntVector sourceGridPos = thing.GridPos;

        gridManager.DeregisterGridPos(thing, thing.GridPos);
        thing.SetGridPos(targetGridPos);

        if (targetThing != null)
        {
            gridManager.DeregisterGridPos(targetThing, targetThing.GridPos);
            targetThing.SetGridPos(sourceGridPos);
        }

        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType);
    }

    public void GridCellClicked(IntVector gridPos, GridType gridType, bool rightClick, bool shift, bool doubleClick, bool visible = true)
    {
        if (gridType == GridType.Arena)
        {
            //RoguemojiGame.Instance.AddFloater("💢", gridPos, 1.5f, CurrentLevelId, new Vector2(15f, -8f), new Vector2(15, -10f), "", requireSight: true, EasingType.ExpoIn, 0.1f, 0.75f, parent: this);
            //RoguemojiGame.Instance.AddFloater("❗️", gridPos, 1f, CurrentLevelId, new Vector2(0f, -25f), new Vector2(0, -35f), "", requireSight: true, EasingType.Linear, 0.1f, 1.1f, parent: this);
            //RoguemojiGame.Instance.AddFloater("❔", gridPos, 1.1f, CurrentLevelId, new Vector2(0f, -29f), new Vector2(0, -33f), "", requireSight: true, EasingType.SineIn, 0.25f, 1f, parent: this);

            var level = RoguemojiGame.Instance.Levels[CurrentLevelId];
            var thing = level.GridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (!visible && thing != null)
                return;

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

    public void InventoryThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos, bool draggedWieldedThing)
    {
        if (destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.Nearby || destinationPanelType == PanelType.None)
        {
            MoveThingTo(thing, GridType.Arena, GridPos);
        }
        else if (destinationPanelType == PanelType.InventoryGrid)
        {
            if(draggedWieldedThing)
            {
                if (thing.GridPos.Equals(targetGridPos))
                {
                    WieldThing(null);
                }
                else
                {
                    var targetThing = InventoryGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
                    WieldThing(targetThing == null || targetThing.Flags.HasFlag(ThingFlags.Equipment) ? null : targetThing);
                    SwapGridThingPos(thing, GridType.Inventory, targetGridPos);
                }
            }
            else
            {
                if (!thing.GridPos.Equals(targetGridPos))
                    SwapGridThingPos(thing, GridType.Inventory, targetGridPos);
                else
                    SelectThing(thing);
            }
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
        else if (destinationPanelType == PanelType.Info)
        {
            SelectThing(thing);
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
                SwapGridThingPos(thing, GridType.Equipment, targetGridPos);
            else
                SelectThing(thing);
        }
        else if (destinationPanelType == PanelType.Info)
        {
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
                MoveThingTo(thing, GridType.Inventory, emptyGridPos);
                WieldThing(thing, dontRequireAction: true);
            }
        }
        else if (destinationPanelType == PanelType.PlayerIcon)
        {
            // todo
        }
        else if (destinationPanelType == PanelType.Info)
        {
            SelectThing(thing);
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

    public void CharacterHotkeyPressed()
    {
        SelectThing(this);
    }

    public GridManager GetGridManager(GridType gridType)
    {
        switch (gridType)
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

    public override void OnChangedStat(StatType statType)
    {
        if (statType == StatType.Sight)
            RefreshVisibility();
    }

    public void StartAimingWieldedItem(int range)
    {
        if (WieldedThing == null)
            return;

        StartAiming(AimingSource.Throwing, range);
        //RoguemojiGame.Instance.LogMessageClient(To.Single(this), "Press WASD to throw or F to cancel.", playerNum: 0);
    }

    public void StartAiming(AimingSource aimingSource, int range)
    {
        IsAiming = true;
        AimingSource = aimingSource;
        StartAimingClient(To.Single(this), range);
    }

    [ClientRpc]
    public void StartAimingClient(int range)
    {
        AimingCells.Clear();

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (dir == Direction.None) continue;

            for(int i = 0; i <= range; i++)
            {
                IntVector gridPos = GridPos + GridManager.GetIntVectorForDirection(dir) * i;
                AimingCells.Add(gridPos);
            }
        }
    }

    public void ConfirmAiming(Direction direction)
    {
        if (!IsAiming || AimingType != AimingType.Direction)
            return;

        StopAiming();

        if(AimingSource == AimingSource.Throwing)
            ThrowWieldedThing(direction);
    }

    public void ConfirmAiming(IntVector gridPos)
    {
        var direction = GridManager.GetDirectionForIntVector(gridPos - GridPos);
        ConfirmAiming(direction);
    }

    public void StopAiming()
    {
        IsAiming = false;
    }

    [ClientRpc]
    public void RefreshVisibility()
    {
        ComputeVisibility(GridPos, rangeLimit: GetStatClamped(StatType.Sight));
    }

    public bool IsCellVisible(IntVector gridPos)
    {
        return VisibleCells.Contains(gridPos);
    }

    public void SetCellVisible(int x, int y)
    {
        IntVector gridPos = new IntVector(x, y);

        if (!ContainingGridManager.IsGridPosInBounds(gridPos))
            return;

        VisibleCells.Add(gridPos);
    }

    public void ClearCellVisibility()
    {
        VisibleCells.Clear();
    }

    public void ComputeVisibility(IntVector origin, int rangeLimit)
    {
        ClearCellVisibility();

        SetCellVisible(origin.x, origin.y);
        for (uint octant = 0; octant < 8; octant++) 
            Compute(octant, origin, rangeLimit, 1, new Slope(1, 1), new Slope(0, 1));
    }

    struct Slope // represents the slope Y/X as a rational number
    {
        public Slope(uint y, uint x) { Y = y; X = x; }

        public bool Greater(uint y, uint x) { return Y * x > X * y; } // this > y/x
        public bool GreaterOrEqual(uint y, uint x) { return Y * x >= X * y; } // this >= y/x
        public bool Less(uint y, uint x) { return Y * x < X * y; } // this < y/x
        public bool LessOrEqual(uint y, uint x) { return Y * x <= X * y; } // this <= y/x

        public readonly uint X, Y;
    }

    // http://www.adammil.net/blog/v125_roguelike_vision_algorithms.html#mycode
    void Compute(uint octant, IntVector origin, int rangeLimit, uint x, Slope top, Slope bottom)
    {
        for (; x <= (uint)rangeLimit; x++)
        {
            uint topY;
            if (top.X == 1)
            {
                topY = x;
            }
            else
            {
                topY = ((x * 2 - 1) * top.Y + top.X) / (top.X * 2); 
                if (BlocksLight(x, topY, octant, origin))
                {
                    if (top.GreaterOrEqual(topY * 2 + 1, x * 2) && !BlocksLight(x, topY + 1, octant, origin)) topY++;
                }
                else 
                {
                    uint ax = x * 2; // center
                    if (BlocksLight(x + 1, topY + 1, octant, origin)) ax++; // use bottom-right if the tile above and right is a wall
                    if (top.Greater(topY * 2 + 1, ax)) topY++;
                }
            }

            uint bottomY;
            if (bottom.Y == 0)
            {                 
                bottomY = 0;
            }
            else // bottom > 0
            {
                bottomY = ((x * 2 - 1) * bottom.Y + bottom.X) / (bottom.X * 2);
                if (bottom.GreaterOrEqual(bottomY * 2 + 1, x * 2) && BlocksLight(x, bottomY, octant, origin) &&
                   !BlocksLight(x, bottomY + 1, octant, origin))
                {
                    bottomY++;
                }
            }

            // go through the tiles in the column now that we know which ones could possibly be visible
            int wasOpaque = -1; // 0:false, 1:true, -1:not applicable
            for (uint y = topY; (int)y >= (int)bottomY; y--)
            {
                if (rangeLimit < 0 || GetDistance((int)x, (int)y) <= rangeLimit) // skip the tile if it's out of visual range
                {
                    bool isOpaque = BlocksLight(x, y, octant, origin);

                    bool isVisible = (y != topY || top.GreaterOrEqual(y, x)) && (y != bottomY || bottom.LessOrEqual(y, x));
                    
                    if (isVisible) 
                        SetVisible(x, y, octant, origin);

                    if (x != rangeLimit) 
                    {
                        if (isOpaque)
                        {
                            if (wasOpaque == 0) 
                            {                  
                                uint nx = x * 2, ny = y * 2 + 1; 
                                                                 
                                // NOTE: if you're using full symmetry and want more expansive walls (recommended), comment out the next line
                                //if (BlocksLight(x, y + 1, octant, origin)) nx--;

                                if (top.Greater(ny, nx)) 
                                {                       
                                    if (y == bottomY) 
                                    { 
                                        bottom = new Slope(ny, nx); 
                                        break; 
                                    } 
                                    else
                                    {
                                        Compute(octant, origin, rangeLimit, x + 1, top, new Slope(ny, nx));
                                    }
                                }
                                else 
                                {    
                                    if (y == bottomY) 
                                        return;
                                }
                            }
                            wasOpaque = 1;
                        }
                        else
                        {
                            if (wasOpaque > 0) 
                            {
                                uint nx = x * 2, ny = y * 2 + 1; 
                                                                 
                                // NOTE: if you're using full symmetry and want more expansive walls (recommended), comment out the next line
                                //if (BlocksLight(x + 1, y + 1, octant, origin)) nx++;
                                                                                     
                                if (bottom.GreaterOrEqual(ny, nx)) 
                                    return;

                                top = new Slope(ny, nx);
                            }
                            wasOpaque = 0;
                        }
                    }
                }
            }

            if (wasOpaque != 0) 
                break;
        }
    }

    // NOTE: the code duplication between BlocksLight and SetVisible is for performance. don't refactor the octant
    // translation out unless you don't mind an 18% drop in speed
    bool BlocksLight(uint x, uint y, uint octant, IntVector origin)
    {
        uint nx = (uint)origin.x, ny = (uint)origin.y;
        switch (octant)
        {
            case 0: nx += x; ny -= y; break;
            case 1: nx += y; ny -= x; break;
            case 2: nx -= y; ny -= x; break;
            case 3: nx -= x; ny -= y; break;
            case 4: nx -= x; ny += y; break;
            case 5: nx -= y; ny += x; break;
            case 6: nx += y; ny += x; break;
            case 7: nx += x; ny += y; break;
        }
        return BlocksLight((int)nx, (int)ny);
    }

    void SetVisible(uint x, uint y, uint octant, IntVector origin)
    {
        uint nx = (uint)origin.x, ny = (uint)origin.y;
        switch (octant)
        {
            case 0: nx += x; ny -= y; break;
            case 1: nx += y; ny -= x; break;
            case 2: nx -= y; ny -= x; break;
            case 3: nx -= x; ny -= y; break;
            case 4: nx -= x; ny += y; break;
            case 5: nx -= y; ny += x; break;
            case 6: nx += y; ny += x; break;
            case 7: nx += x; ny += y; break;
        }
        SetCellVisible((int)nx, (int)ny);
    }

    int GetDistance(int x, int y)
    {
        return (int)Math.Round(Math.Sqrt(x * x + y * y));
    }
    
    bool BlocksLight(int x, int y)
    {
        Game.AssertClient();

        return ContainingGridManager.GetThingsAtClient(new IntVector(x, y)).Where(x => x.SightBlockAmount >= GetStatClamped(StatType.Sight)).Count() > 0;
    }
}

public interface IQueuedAction
{
    public void Execute(RoguemojiPlayer player);
}

public class TryMoveAction : IQueuedAction
{
    public Direction Direction { get; set; }

    public TryMoveAction(Direction direction)
    {
        Direction = direction;
    }

    public void Execute(RoguemojiPlayer player)
    {
        player.TryMove(Direction, shouldQueueAction: false);
    }

    public override string ToString()
    {
        return $"TryMove {Direction}";
    }
}

public class MoveThingAction : IQueuedAction
{
    public Thing Thing { get; set; }
    public GridType TargetGridType { get; set; }
    public IntVector TargetGridPos { get; set; }
    public GridType SourceGridType { get; set; }
    public IntVector SourceGridPos { get; set; }
    public bool WieldIfPossible { get; set; }

    public MoveThingAction(Thing thing, GridType targetGridType, IntVector targetGridPos, GridType sourceGridType, IntVector sourceGridPos, bool wieldIfPossible = false)
    {
        Thing = thing;
        TargetGridType = targetGridType;
        TargetGridPos = targetGridPos;
        SourceGridType = sourceGridType;
        SourceGridPos = sourceGridPos;
        WieldIfPossible = wieldIfPossible;
    }

    public void Execute(RoguemojiPlayer player)
    {
        if (Thing.ContainingGridManager.GridType != SourceGridType || !Thing.GridPos.Equals(SourceGridPos))
            return;

        player.MoveThingTo(Thing, TargetGridType, TargetGridPos, wieldIfPossible: WieldIfPossible);
    }

    public override string ToString()
    {
        return $"Move {Thing.DisplayName} -> {TargetGridType} in {TargetGridPos}";
    }
}

public class WieldThingAction : IQueuedAction
{
    public Thing Thing { get; set; }

    public WieldThingAction(Thing thing)
    {
        Thing = thing;
    }

    public void Execute(RoguemojiPlayer player)
    {
        if (Thing != null && Thing.ContainingGridManager.OwningPlayer != player)
            return;

        player.WieldThing(Thing);
    }

    public override string ToString()
    {
        return $"Wield {Thing?.DisplayName ?? null}";
    }
}

// todo
public class ThrowThingAction : IQueuedAction
{
    public Thing Thing { get; set; }
    public Direction Direction { get; set; }

    public ThrowThingAction(Thing thing, Direction direction)
    {
        Thing = thing;
        Direction = direction;
    }

    public void Execute(RoguemojiPlayer player)
    {
        if (Thing == null || Thing != player.WieldedThing)
            return;

        player.ThrowWieldedThing(Direction);
    }

    public override string ToString()
    {
        return $"Throw {Thing?.DisplayName ?? null} {Direction}";
    }
}