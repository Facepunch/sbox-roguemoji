using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum AimingSource { Throwing, UsingWieldedItem }
public enum AimingType { Direction, TargetCell }

public partial class RoguemojiPlayer : Thing
{
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

    [Net] public bool IsAiming { get; set; }
    [Net] public AimingSource AimingSource { get; set; }
    [Net] public AimingType AimingType { get; set; }
    public HashSet<IntVector> AimingCells { get; set; } // Client-only

    public RoguemojiPlayer()
	{
		IconDepth = 5;
        ShouldUpdate = true;
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

        Acting = AddComponent<Acting>();
    }

    void SetStartingValues()
    {
        DisplayIcon = "🙂";
        Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanUseThings;
        IsDead = false;
        CurrentLevelId = LevelId.None;
        //ActionDelay = TimeSinceAction = 0.5f;
        //IsActionReady = true;
        QueuedAction = null;
        QueuedActionName = "";
        RefreshVisibility();
        SightBlockAmount = 10;
        IsAiming = false;
        SelectedThing = null;
        IsRemoved = false;
        IsOnCooldown = false;

        ClearStats();
        InitStat(StatType.Health, 10, 0, 10);
        InitStat(StatType.Energy, 5, 0, 5);
        InitStat(StatType.Mana, 0, 0, 0);
        InitStat(StatType.Attack, 1);
        InitStat(StatType.Strength, 2);
        InitStat(StatType.Speed, 5);
        InitStat(StatType.Intelligence, 5);
        InitStat(StatType.Charisma, 3);
        InitStat(StatType.Sight, 9);
        InitStat(StatType.Hearing, 3);
        //InitStat(StatType.Smell, 1);
        FinishInitStats();

        ClearTraits();

        InventoryGridManager.Restart();
        InventoryGridManager.SetWidth(RoguemojiGame.InventoryWidth);

        EquipmentGridManager.Restart();

        for (int x = 0; x < RoguemojiGame.InventoryWidth - 1; x++)
            for (int y = 0; y < RoguemojiGame.InventoryHeight - 1; y++)
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
        int rand = Game.Random.Int(0, 20);
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
            case 12: InventoryGridManager.SpawnThing<ScrollBlink>(gridPos); break;
            case 13: InventoryGridManager.SpawnThing<BowAndArrow>(gridPos); break;
            case 14: InventoryGridManager.SpawnThing<Backpack>(gridPos); break;
            case 15: InventoryGridManager.SpawnThing<BookBlink>(gridPos); break;
            case 16: InventoryGridManager.SpawnThing<PotionMana>(gridPos); break;
            case 17: InventoryGridManager.SpawnThing<PotionHealth>(gridPos); break;
            case 18: InventoryGridManager.SpawnThing<PotionEnergy>(gridPos); break;
            case 19: InventoryGridManager.SpawnThing<ScrollTeleport>(gridPos); break;
            case 20: InventoryGridManager.SpawnThing<BookTeleport>(gridPos); break;
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

    public override void Update(float dt)
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
            if (!IsDead)
            {
                if (!IsAiming)
                {
                    if (Input.Pressed(InputButton.Slot1))                                                       WieldHotbarSlot(0);
                    else if (Input.Pressed(InputButton.Slot2))                                                  WieldHotbarSlot(1);
                    else if (Input.Pressed(InputButton.Slot3))                                                  WieldHotbarSlot(2);
                    else if (Input.Pressed(InputButton.Slot4))                                                  WieldHotbarSlot(3);
                    else if (Input.Pressed(InputButton.Slot5))                                                  WieldHotbarSlot(4);
                    else if (Input.Pressed(InputButton.Slot6))                                                  WieldHotbarSlot(5);
                    else if (Input.Pressed(InputButton.Slot7))                                                  WieldHotbarSlot(6);
                    else if (Input.Pressed(InputButton.Slot8))                                                  WieldHotbarSlot(7);
                    else if (Input.Pressed(InputButton.Slot9))                                                  WieldHotbarSlot(8);
                    else if (Input.Pressed(InputButton.Slot0))                                                  WieldHotbarSlot(9);
                    else if (Input.Pressed(InputButton.Use))                                                    PickUpTopItem();
                    else if (Input.Pressed(InputButton.Drop))                                                   DropWieldedItem();
                    else if (Input.Pressed(InputButton.Jump))                                                   UseWieldedThing();
                    else if (Input.Pressed(InputButton.Menu))                                                   WieldThing(null);
                    else if (Input.Pressed(InputButton.View))                                                   CharacterHotkeyPressed();
                    else if (Input.Pressed(InputButton.Left))                                                   TryMove(Direction.Left, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Right))                                                  TryMove(Direction.Right, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Back))                                                   TryMove(Direction.Down, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Forward))                                                TryMove(Direction.Up, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Flashlight))                                             StartAimingThrow();
                    else if (Input.Down(InputButton.Left))                                                      TryMove(Direction.Left);
                    else if (Input.Down(InputButton.Right))                                                     TryMove(Direction.Right);
                    else if (Input.Down(InputButton.Back))                                                      TryMove(Direction.Down);
                    else if (Input.Down(InputButton.Forward))                                                   TryMove(Direction.Up);
                }
                else
                {
                    if (Input.Pressed(InputButton.Left))                                                        ConfirmAiming(Direction.Left);
                    else if (Input.Pressed(InputButton.Right))                                                  ConfirmAiming(Direction.Right);
                    else if (Input.Pressed(InputButton.Back))                                                   ConfirmAiming(Direction.Down);
                    else if (Input.Pressed(InputButton.Forward))                                                ConfirmAiming(Direction.Up);
                    else if (Input.Pressed(InputButton.Jump) && AimingSource == AimingSource.UsingWieldedItem)  StopAiming();
                    else if (Input.Pressed(InputButton.Flashlight) && AimingSource == AimingSource.Throwing)    StopAiming();
                }
            }

            if (Input.Pressed(InputButton.Reload))
            {
                RoguemojiGame.Instance.Restart();
            }
        }
	}
    
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
        if (index >= InventoryGridManager.GridWidth)
            return;

        var thing = InventoryGridManager.GetThingsAt(InventoryGridManager.GetGridPos(index)).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing != null && Input.Down(InputButton.Run))
        {
            MoveThingTo(thing, GridType.Arena, GridPos);
        }
        else
        {
            if(thing != null && thing.Flags.HasFlag(ThingFlags.Equipment))
                TryEquipThing(thing);
            else
                WieldThing(thing);
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

    public void RecenterCamera()
    {
        var middleCell = new IntVector(MathX.FloorToInt((float)RoguemojiGame.ArenaWidth / 2f), MathX.FloorToInt((float)RoguemojiGame.ArenaHeight / 2f));
        SetCameraGridOffset(GridPos - middleCell);
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

        var projectile = WieldedThing.AddComponent<Projectile>();
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

    void TryEquipThing(Thing thing)
    {
        if (EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            MoveThingTo(thing, GridType.Equipment, emptyGridPos);
    }

    public override void UseWieldedThing()
    {
        if (WieldedThing == null)
        {
            if (SelectedThing != null && IsInInventory(SelectedThing))
            {
                if (SelectedThing.Flags.HasFlag(ThingFlags.Equipment))
                    TryEquipThing(SelectedThing);
                else
                    WieldThing(SelectedThing);
            }

            return;
        }

        if (!WieldedThing.Flags.HasFlag(ThingFlags.Useable))
            return;

        if (WieldedThing.IsOnCooldown)
            return;
            
        if(!WieldedThing.TryStartUsing(this))
            return;

        if (WieldedThing.Flags.HasFlag(ThingFlags.UseRequiresAiming))
        {
            AimingType aimingType = WieldedThing.Flags.HasFlag(ThingFlags.AimTypeTargetCell) ? AimingType.TargetCell : AimingType.Direction;
            StartAiming(AimingSource.UsingWieldedItem, aimingType);
        }
        else
        {
            if (!Acting.IsActionReady)
            {
                QueuedAction = new UseWieldedThingAction();
                QueuedActionName = QueuedAction.ToString();
                return;
            }

            WieldedThing.Use(this);
        }
    }

    public override void UseWieldedThing(Direction direction)
    {
        if (!Acting.IsActionReady)
        {
            QueuedAction = new UseWieldedThingDirectionAction(direction);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        base.UseWieldedThing(direction);
    }

    public override void UseWieldedThing(IntVector targetGridPos)
    {
        if (!Acting.IsActionReady)
        {
            QueuedAction = new UseWieldedThingTargetAction(targetGridPos);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        base.UseWieldedThing(targetGridPos);
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

        if (sourceGridType == GridType.Equipment)
            thing.ContainingGridManager.OwningPlayer.UnequipThing(thing);

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

            if (thing != null && (doubleClick || rightClick))
            {
                if (thing.Flags.HasFlag(ThingFlags.Equipment))
                    TryEquipThing(thing);
                else
                    WieldThing(thing);
            }
            else if (!rightClick)
            {
                if (thing != null && shift)
                    MoveThingTo(thing, GridType.Arena, GridPos);
                else
                    SelectThing(thing);
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
                TryEquipThing(thing);
            else
                WieldThing(thing);
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

    public override void OnChangedStat(StatType statType, int changeCurrent, int changeMin, int changeMax)
    {
        base.OnChangedStat(statType, changeCurrent, changeMin, changeMax);

        if (statType == StatType.Sight)
        {
            RefreshVisibility(To.Single(this));
        }
        else if(statType == StatType.Speed)
        {
            Acting.ActionDelay = Acting.CalculateActionDelay(GetStatClamped(StatType.Speed));
        }
        else if(statType == StatType.Intelligence)
        {
            AdjustStatMax(StatType.Mana, changeCurrent);
            AdjustStat(StatType.Mana, changeCurrent);
        }
    }

    public void StartAimingThrow()
    {
        if (WieldedThing == null)
            return;

        StartAiming(AimingSource.Throwing, AimingType.Direction);
        //RoguemojiGame.Instance.LogMessageClient(To.Single(this), "Press WASD to throw or F to cancel.", playerNum: 0);
    }

    public void StartAiming(AimingSource aimingSource, AimingType aimingType)
    {
        if (QueuedAction != null)
        {
            QueuedAction = null;
            QueuedActionName = "";
        }

        IsAiming = true;
        AimingSource = aimingSource;
        AimingType = aimingType;

        if(aimingType == AimingType.Direction)
        {
            AimDirectionClient(To.Single(this));
        }
        else if(aimingSource == AimingSource.UsingWieldedItem && aimingType == AimingType.TargetCell)
        {
            if(WieldedThing != null)
                AimTargetCellsClient(To.Single(this), WieldedThing.NetworkIdent);
            else
                StopAiming();
        }
    }

    [ClientRpc]
    public void AimDirectionClient()
    {
        AimingCells.Clear();

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            IntVector gridPos = GridPos + GridManager.GetIntVectorForDirection(dir);
            AimingCells.Add(gridPos);
        }
    }

    public void RefreshWieldedThingTargetAiming()
    {
        if (WieldedThing == null || !IsAiming || AimingSource != AimingSource.UsingWieldedItem || AimingType != AimingType.TargetCell)
            return;

        AimTargetCellsClient(To.Single(this), WieldedThing.NetworkIdent);
    }

    [ClientRpc]
    public void AimTargetCellsClient(int networkIdent)
    {
        AimingCells.Clear();

        Thing usedThing = FindByIndex(networkIdent) as Thing;
        var aimingCells = usedThing.GetAimingTargetCellsClient();

        foreach (IntVector gridPos in aimingCells)
            AimingCells.Add(gridPos);
    }

    public void ConfirmAiming(Direction direction)
    {
        if (!IsAiming || AimingType != AimingType.Direction)
            return;

        StopAiming();

        if (AimingSource == AimingSource.Throwing)
            ThrowWieldedThing(direction);
        else if (AimingSource == AimingSource.UsingWieldedItem)
            UseWieldedThing(direction);
    }

    public void ConfirmAiming(IntVector gridPos)
    {
        if (!IsAiming)
            return;

        if(AimingType == AimingType.Direction)
        {
            var direction = GridManager.GetDirectionForIntVector(gridPos - GridPos);
            ConfirmAiming(direction);
        }
        else if(AimingType == AimingType.TargetCell && AimingSource == AimingSource.UsingWieldedItem)
        {
            UseWieldedThing(gridPos);
            StopAiming();
        }
    }

    public void StopAiming()
    {
        IsAiming = false;
    }

    public override void OnChangedGridPos()
    {
        base.OnChangedGridPos();

        RefreshVisibility(To.Single(this));

        //InventoryGridManager.SetWidth(InventoryGridManager.GridWidth - 1);
        //var nearbyThings = ContainingGridManager.GetThingsWithinRange(GridPos, 2, allFlags: ThingFlags.Solid);
        //foreach(var thing in nearbyThings)
        //{
        //    if (thing == this)
        //        continue;

        //    RoguemojiGame.Instance.DebugGridLine(GridPos, thing.GridPos, Color.Red, 0.1f, ContainingGridManager.LevelId);
        //    RoguemojiGame.Instance.DebugGridCell(thing.GridPos, Color.Red, 1f, ContainingGridManager.LevelId);
        //}
    }

    public bool IsInInventory(Thing thing)
    {
        return thing.ContainingGridManager.GridType == GridType.Inventory && thing.ContainingGridManager.OwningPlayer == this;
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

public class UseWieldedThingAction : IQueuedAction
{
    public void Execute(RoguemojiPlayer player)
    {
        player.UseWieldedThing();
    }

    public override string ToString()
    {
        return $"UseWieldedThing";
    }
}

public class UseWieldedThingDirectionAction : IQueuedAction
{
    public Direction Direction { get; set; }

    public UseWieldedThingDirectionAction(Direction direction)
    {
        Direction = direction;
    }

    public void Execute(RoguemojiPlayer player)
    {
        player.UseWieldedThing(Direction);
    }

    public override string ToString()
    {
        return $"UseWieldedThing {Direction}";
    }
}

public class UseWieldedThingTargetAction : IQueuedAction
{
    public IntVector TargetGridPos { get; set; }

    public UseWieldedThingTargetAction(IntVector targetGridPos)
    {
        TargetGridPos = targetGridPos;
    }

    public void Execute(RoguemojiPlayer player)
    {
        player.UseWieldedThing(TargetGridPos);
    }

    public override string ToString()
    {
        return $"UseWieldedThing {TargetGridPos}";
    }
}
