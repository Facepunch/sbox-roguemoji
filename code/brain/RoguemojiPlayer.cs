using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum AimingSource { Throwing, UsingWieldedItem }
public enum AimingType { Direction, TargetCell }

public partial class RoguemojiPlayer : ThingBrain
{
    [Net] public int PlayerNum { get; set; }
    [Net] public IntVector CameraGridOffset { get; set; }
    public Vector2 CameraPixelOffset { get; set; } // Client-only
    public float CameraFade { get; set; } // Client-only

    [Net] public GridManager InventoryGridManager { get; private set; }
    [Net] public GridManager EquipmentGridManager { get; private set; }

    [Net] public Thing SelectedThing { get; private set; }

    public Dictionary<TypeDescription, PlayerComponent> PlayerComponents = new Dictionary<TypeDescription, PlayerComponent>();

    public IQueuedAction QueuedAction { get; private set; }
    [Net] public string QueuedActionName { get; private set; }

    [Net] public bool IsAiming { get; set; }
    [Net] public AimingSource AimingSource { get; set; }
    [Net] public AimingType AimingType { get; set; }
    [Net] public GridType AimingGridType { get; set; }
    public HashSet<IntVector> AimingCells { get; set; } // Client-only

    [Net] public IList<ScrollType> IdentifiedScrollTypes { get; private set; }
    [Net] public IList<PotionType> IdentifiedPotionTypes { get; private set; }

    [Net] public int ConfusionSeed { get; set; }
    public bool IsConfused => ConfusionSeed > 0;

    [Net] public int HallucinatingSeed { get; set; }
    public bool IsHallucinating => HallucinatingSeed > 0;

    public RoguemojiPlayer()
	{
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

            IdentifiedScrollTypes = new List<ScrollType>();
            IdentifiedPotionTypes = new List<PotionType>();
            SetStartingValues();
        }
        else
        {
            VisibleCells = new HashSet<IntVector>();
            SeenCells = new Dictionary<LevelId, HashSet<IntVector>>();
            SeenThings = new Dictionary<LevelId, Dictionary<IntVector, List<SeenThingData>>>();
            AimingCells = new HashSet<IntVector>();
        }
	}

    void SetStartingValues()
    {
        QueuedAction = null;
        QueuedActionName = "";
        IsAiming = false;
        SelectedThing = null;
        CameraFade = 0f;
        ConfusionSeed = 0;
        HallucinatingSeed = 0;

        IdentifiedScrollTypes.Clear();
        IdentifiedPotionTypes.Clear();

        // -----------------
        foreach (int i in Enum.GetValues(typeof(ScrollType)))
            IdentifiedScrollTypes.Add((ScrollType)i);

        foreach (int i in Enum.GetValues(typeof(PotionType)))
            IdentifiedPotionTypes.Add((PotionType)i);
        // -----------------

        InventoryGridManager.Restart();
        InventoryGridManager.SetWidth(RoguemojiGame.InventoryWidth);

        EquipmentGridManager.Restart();

        for (int x = 0; x < RoguemojiGame.InventoryWidth; x++)
            for (int y = 0; y < RoguemojiGame.InventoryHeight; y++)
                SpawnRandomInventoryThing(new IntVector(x, y));

        RoguemojiGame.Instance.RefreshGridPanelClient(GridType.Inventory);
        RoguemojiGame.Instance.RefreshGridPanelClient(GridType.Equipment);
        RoguemojiGame.Instance.RefreshNearbyPanelClient();
    }

    public override void ControlThing(Thing thing)
    {
        base.ControlThing(thing);

        thing.PlayerNum = PlayerNum;

        if (Client == null)
            return;

        thing.DisplayName = $"{Client.Name}";
        thing.Tooltip = $"{Client.Name}";
    }

    public void Restart()
    {
        SetStartingValues();
        //RestartClient();
    }

    [ClientRpc]
    public void RestartClient()
    {
        VisibleCells.Clear();
        SeenCells.Clear();
        SeenThings.Clear();

        InventoryGridManager.Floaters.Clear();
        EquipmentGridManager.Floaters.Clear();
    }

    [ClientRpc]
    public void ClearVisionKnowledgeClient()
    {
        SeenCells.Clear();
        SeenThings.Clear();
    }

    void SpawnRandomInventoryThing(IntVector gridPos)
    {
        int rand = Game.Random.Int(0, 31);
        switch (rand)
        {
            //case 0: InventoryGridManager.SpawnThing<Leaf>(gridPos); break;
            case 0: InventoryGridManager.SpawnThing<PotionMedicine>(gridPos); break;
            //case 1: InventoryGridManager.SpawnThing<Potato>(gridPos); break;
            case 1: InventoryGridManager.SpawnThing<PotionMutation>(gridPos); break;
            //case 2: InventoryGridManager.SpawnThing<Nut>(gridPos); break;
            case 2: InventoryGridManager.SpawnThing<PotionHallucination>(gridPos); break;
            //case 3: InventoryGridManager.SpawnThing<Mushroom>(gridPos); break;
            case 3: InventoryGridManager.SpawnThing<ScrollDisplace>(gridPos); break;
            //case 4: InventoryGridManager.SpawnThing<Trumpet>(gridPos); break;
            case 4: InventoryGridManager.SpawnThing<PotionConfusion>(gridPos); break;
            //case 5: InventoryGridManager.SpawnThing<Bouquet>(gridPos); break;
            case 5: InventoryGridManager.SpawnThing<PotionPoison>(gridPos); break;
            //case 6: InventoryGridManager.SpawnThing<Cheese>(gridPos); break;
            case 6: InventoryGridManager.SpawnThing<PotionBlindness>(gridPos); break;
            //case 7: InventoryGridManager.SpawnThing<Coat>(gridPos); break;
            case 7: InventoryGridManager.SpawnThing<ScrollTelekinesis>(gridPos); break;
            //case 8: InventoryGridManager.SpawnThing<SafetyVest>(gridPos); break;
            case 8: InventoryGridManager.SpawnThing<PotionSpeed>(gridPos); break;
            //case 9: InventoryGridManager.SpawnThing<Sunglasses>(gridPos); break;
            case 9: InventoryGridManager.SpawnThing<GlassesOfPerception>(gridPos); break;
            //case 10: InventoryGridManager.SpawnThing<Telescope>(gridPos); break;
            //case 10: InventoryGridManager.SpawnThing<Refreshment>(gridPos); break;
            case 10: InventoryGridManager.SpawnThing<PotionSleeping>(gridPos); break;
            //case 11: InventoryGridManager.SpawnThing<WhiteCane>(gridPos); break;
            case 11: InventoryGridManager.SpawnThing<ScrollConfetti>(gridPos); break;
            //case 11: InventoryGridManager.SpawnThing<Cigarette>(gridPos); break;
            case 12: InventoryGridManager.SpawnThing<ScrollBlink>(gridPos); break;
            //case 13: InventoryGridManager.SpawnThing<BowAndArrow>(gridPos); break;
            case 13: InventoryGridManager.SpawnThing<ScrollIdentify>(gridPos); break;
            case 14: InventoryGridManager.SpawnThing<Backpack>(gridPos); break;
            //case 14: InventoryGridManager.SpawnThing<Joystick>(gridPos); break;
            //case 14: InventoryGridManager.SpawnThing<Juicebox>(gridPos); break;
            case 15: InventoryGridManager.SpawnThing<BookBlink>(gridPos); break;
            case 16: InventoryGridManager.SpawnThing<PotionMana>(gridPos); break;
            case 17: InventoryGridManager.SpawnThing<PotionHealth>(gridPos); break;
            //case 18: InventoryGridManager.SpawnThing<PotionEnergy>(gridPos); break;
            case 18: InventoryGridManager.SpawnThing<Axe>(gridPos); break;
            case 19: InventoryGridManager.SpawnThing<ScrollTeleport>(gridPos); break;
            case 20: InventoryGridManager.SpawnThing<BookTeleport>(gridPos); break;
            case 21: InventoryGridManager.SpawnThing<RugbyBall>(gridPos); break;
            case 22: InventoryGridManager.SpawnThing<PotionInvisible>(gridPos); break;
            case 23: InventoryGridManager.SpawnThing<ScrollFear>(gridPos); break;
            case 24: InventoryGridManager.SpawnThing<ScrollOrganize>(gridPos); break;
            case 25: InventoryGridManager.SpawnThing<BookOrganize>(gridPos); break;
            case 26: InventoryGridManager.SpawnThing<PotionAmnesia>(gridPos); break;
            case 27: InventoryGridManager.SpawnThing<PotionBurning>(gridPos); break;
            case 28: InventoryGridManager.SpawnThing<AcademicCap>(gridPos); break;
            case 29: InventoryGridManager.SpawnThing<Basketball>(gridPos); break;
            case 30: InventoryGridManager.SpawnThing<ScrollSentience>(gridPos); break;
            case 31: InventoryGridManager.SpawnThing<PotionWater>(gridPos); break;
        }
    }

    public override void OnClientActive(IClient client)
    {
        base.OnClientActive(client);

        //DisplayName = Client.Name;
        //Tooltip = Client.Name;
	}

    public override void Update(float dt)
	{
		base.Update(dt);

        //RoguemojiGame.Instance.DebugGridCell(GridPos + new IntVector(0, -1), new Color(1f, 0f, 0f, 0.3f), 0.05f);
        //RoguemojiGame.Instance.DebugGridLine(GridPos + new IntVector(0, -1), new IntVector(3, 2), new Color(1f, 0f, 0f, 0.3f), 0.05f, GridType.Arena, GridType.Inventory);
        //RoguemojiGame.Instance.DebugGridCell(new IntVector(3, 2), new Color(1f, 0f, 0f, 0.3f), 0.05f, GridType.Inventory);

        InventoryGridManager.Update(dt);
        EquipmentGridManager.Update(dt);

        for (int i = PlayerComponents.Count - 1; i >= 0; i--)
        {
            KeyValuePair<TypeDescription, PlayerComponent> pair = PlayerComponents.ElementAt(i);

            var component = pair.Value;
            if (component.ShouldUpdate)
                component.Update(dt);
        }

        if (ControlledThing == null)
            return;

        if (SelectedThing != null && !ControlledThing.CanPerceiveThing(SelectedThing))
            SelectThing(null);

        //ControlledThing.DebugText = "...";
        //if (QueuedAction != null)
        //    ControlledThing.DebugText = QueuedActionName;
    }

	public override void Simulate(IClient cl )
	{
		if(Game.IsServer)
		{
            if (ControlledThing == null)
                return;

            if (Input.Pressed(InputButton.View)) 
                CharacterHotkeyPressed();

            if (!ControlledThing.IsInTransit)
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
                    else if (Input.Pressed(InputButton.Left))                                                   TryMove(Direction.Left, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Right))                                                  TryMove(Direction.Right, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Back))                                                   TryMove(Direction.Down, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Forward))                                                TryMove(Direction.Up, shouldQueueAction: true);
                    else if (Input.Pressed(InputButton.Flashlight))                                             StartAimingThrow();
                    else if (Input.Pressed(InputButton.Voice))                                                  SelectWieldedItem();
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

        InventoryGridManager.UpdateClient(dt);
        EquipmentGridManager.UpdateClient(dt);
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

    public void ClearQueuedAction()
    {
        QueuedAction = null;
    }

    void WieldHotbarSlot(int index)
    {
        if (index >= InventoryGridManager.GridWidth)
            return;

        var thing = InventoryGridManager.GetThingsAt(InventoryGridManager.GetGridPos(index)).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing != null && Input.Down(InputButton.Run))
        {
            MoveThingTo(thing, GridType.Arena, ControlledThing.GridPos, playSfx: true);
        }
        else
        {
            if(thing != null && thing.HasFlag(ThingFlags.Equipment))
                TryEquipThing(thing);
            else
                WieldThing(thing);
        }
    }

    public override void OnWieldThing(Thing thing) 
    {
        RoguemojiGame.Instance.FlickerWieldingPanel();
    }

    public bool TryMove(Direction direction, bool shouldAnimate = true, bool shouldQueueAction = false, bool dontRequireAction = false)
	{
        if (ControlledThing.IsInTransit)
            return false;

        CActing acting = null;
        if (ControlledThing.GetComponent<CActing>(out var component))
            acting = (CActing)component;

        if (acting == null)
            return false;

        if (!acting.IsActionReady && !dontRequireAction)
        {
            if(shouldQueueAction)
            {
                QueuedAction = new TryMoveAction(direction);
                QueuedActionName = QueuedAction.ToString();
            }
            
            return false;
        }

        //if(IsConfused && Game.Random.Int(0, 2) == 0)
        //    direction = GridManager.GetRandomDirection(cardinalOnly: false);

        bool success = ControlledThing.TryMove(direction, out bool switchedLevel, shouldAnimate, shouldQueueAction: false, dontRequireAction);

        if (success && !switchedLevel)
            RecenterCamera(shouldAnimate: true);

        return success;
	}

    //public override void BumpInto(Thing other, Direction direction)
    //{
    //    base.BumpInto(other, direction);

    //    //if(other is Hole)
    //    //{
    //    //    if(CurrentLevelId == LevelId.Forest0)
    //    //        RoguemojiGame.Instance.SetPlayerLevel(this, LevelId.Forest1);
    //    //    else if (CurrentLevelId == LevelId.Forest1)
    //    //        RoguemojiGame.Instance.SetPlayerLevel(this, LevelId.Forest2);
    //    //}
    //    if(other is Door)
    //    {
    //        if (Smiley.CurrentLevelId == LevelId.Forest1)
    //            RoguemojiGame.Instance.ChangeThingLevel(Smiley, LevelId.Forest0);
    //        else if (Smiley.CurrentLevelId == LevelId.Forest2)
    //            RoguemojiGame.Instance.ChangeThingLevel(Smiley, LevelId.Forest1);
    //    }
    //}

	public void SelectThing(Thing thing, bool playSfx = false)
	{
		if (SelectedThing == thing)
			return;

		if (SelectedThing != null)
			SelectedThing.RefreshGridPanelClient();

		SelectedThing = thing;

        if (playSfx && thing != null)
        {
            //thing.GetSound(SoundActionType.Select, SurfaceType.None, out string sfxName, out int loudness);
            //RoguemojiGame.Instance.PlaySfxArena(sfxName, ControlledThing.GridPos, ControlledThing.CurrentLevelId, loudness);
            PlaySfxUI("click");
        }
	}

    /// <summary>Returns true if offset changed.</summary>
    public bool RecenterCamera(bool shouldAnimate = false)
    {
        var middleCell = new IntVector(MathX.FloorToInt((float)RoguemojiGame.ArenaPanelWidth / 2f), MathX.FloorToInt((float)RoguemojiGame.ArenaPanelHeight / 2f));
        var oldCamGridOffset = CameraGridOffset;
        var movedCamera = SetCameraGridOffset(ControlledThing.GridPos - middleCell);

        if(movedCamera && shouldAnimate)
        {
            // todo: make an option to turn this off
            var dir = GridManager.GetDirectionForIntVector(CameraGridOffset - oldCamGridOffset);
            VfxSlideCamera(dir, 0.25f, RoguemojiGame.CellSize);
        }

        return movedCamera;
    }

    /// <summary>Returns true if offset changed.</summary>
    public bool SetCameraGridOffset(IntVector offset)
    {
        var currOffset = CameraGridOffset;

        var gridManager = ControlledThing.ContainingGridManager;

        var gridW = gridManager.GridWidth;
        var gridH = gridManager.GridHeight;
        var screenW = RoguemojiGame.ArenaPanelWidth;
        var screenH = RoguemojiGame.ArenaPanelHeight;

        CameraGridOffset = new IntVector(
            gridW >= screenW ? Math.Clamp(offset.x, 0, gridManager.GridWidth - RoguemojiGame.ArenaPanelWidth) : -(screenW - gridW) / 2,
            gridH >= screenH ? Math.Clamp(offset.y, 0, gridManager.GridHeight - RoguemojiGame.ArenaPanelHeight) : -(screenH - gridH) / 2
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
            (gridPos.x < CameraGridOffset.x + RoguemojiGame.ArenaPanelWidth + 1) &&
            (gridPos.y >= CameraGridOffset.y - 1) &&
            (gridPos.y < CameraGridOffset.y + RoguemojiGame.ArenaPanelHeight + 1);
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

    public T AddPlayerComponent<T>() where T : PlayerComponent
    {
        return AddPlayerComponent(TypeLibrary.GetType(typeof(T))) as T;
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

    public void RemovePlayerComponent<T>() where T : PlayerComponent
    {
        RemovePlayerComponent(TypeLibrary.GetType(typeof(T)));
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
        var slide = AddPlayerComponent<VfxPlayerSlideCamera>();
        slide.Direction = direction;
        slide.Lifetime = lifetime;
        slide.Distance = distance;
    }

    [ClientRpc]
    public void VfxShakeCamera(float lifetime, float distance)
    {
        var shake = AddPlayerComponent<VfxPlayerShakeCamera>(); ;
        shake.Lifetime = lifetime;
        shake.Distance = distance;
    }

    [ClientRpc]
    public void VfxFadeCamera(float lifetime, bool shouldFadeOut)
    {
        var fade = AddPlayerComponent<VfxPlayerFadeCamera>();
        fade.Lifetime = lifetime;
        fade.ShouldFadeOut = shouldFadeOut;
    }

    public override void OnDestroyed()
    {
        base.OnDestroyed();

        StopAiming();
        DropAllItems();
        QueuedAction = null;

        ConfusionSeed = 0;
        HallucinatingSeed = 0;

        var ghost = ControlledThing.ContainingGridManager.SpawnThing<Ghost>(ControlledThing.GridPos);
        ControlThing(ghost);
        ghost.DisplayName = $"Ghost of {Client.Name}";
        ghost.Tooltip = $"ghost of {Client.Name}";
    }

    public void PickUpTopItem()
    {
        var thing = ControlledThing.ContainingGridManager.GetThingsAt(ControlledThing.GridPos).WithAll(ThingFlags.CanBePickedUp).WithNone(ThingFlags.Solid).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        TryPickUp(thing);
    }

    public bool TryPickUp(Thing thing, bool dontRequireAction = true)
    {
        if (thing == null)
            return false;

        if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
        {
            MoveThingTo(thing, GridType.Inventory, emptyGridPos, dontRequireAction, wieldIfPossible: true, playSfx: true);
            return true;
        }
        else if (thing.HasFlag(ThingFlags.Equipment) && EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPosEquipment))
        {
            MoveThingTo(thing, GridType.Equipment, emptyGridPosEquipment, dontRequireAction, playSfx: true);
            return true;
        }

        return false;
    }

    public void ThrowWieldedThing(Direction direction)
    {
        CActing acting = null;
        if (ControlledThing.GetComponent<CActing>(out var component))
            acting = (CActing)component;

        if (!acting.IsActionReady)
        {
            QueuedAction = new ThrowThingAction(ControlledThing.WieldedThing, direction);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        if (ControlledThing.WieldedThing == null || direction == Direction.None)
            return;

        var projectile = ControlledThing.WieldedThing.AddComponent<CProjectile>();
        projectile.Direction = direction;
        projectile.MoveDelay = 0.1f;
        projectile.TotalDistance = 5;
        projectile.Thrower = ControlledThing;

        var thing = ControlledThing.WieldedThing;
        MoveThingTo(ControlledThing.WieldedThing, GridType.Arena, ControlledThing.GridPos);
        thing.PlaySfx(SoundActionType.Throw);
    }

    public void DropWieldedItem()
    {
        if (ControlledThing.WieldedThing != null)
        {
            MoveThingTo(ControlledThing.WieldedThing, GridType.Arena, ControlledThing.GridPos, playSfx: true);
        }
    }

    void TryEquipThing(Thing thing)
    {
        if (EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            MoveThingTo(thing, GridType.Equipment, emptyGridPos, playSfx: true);
    }

    void SelectWieldedItem()
    {
        if (ControlledThing.WieldedThing != null)
            SelectThing(ControlledThing.WieldedThing, playSfx: true);
    }

    public void UseWieldedThing()
    {
        Thing wieldedThing = ControlledThing.WieldedThing;
        if (wieldedThing == null)
            return;

        if (!wieldedThing.HasFlag(ThingFlags.Useable))
            return;

        if (wieldedThing.IsOnCooldown)
            return;

        if (!wieldedThing.CanBeUsedBy(ControlledThing, shouldLogMessage: true))
            return;

        if (wieldedThing.HasFlag(ThingFlags.UseRequiresAiming))
        {
            AimingType aimingType = wieldedThing.HasFlag(ThingFlags.AimTypeTargetCell) ? AimingType.TargetCell : AimingType.Direction;
            StartAiming(AimingSource.UsingWieldedItem, aimingType, wieldedThing.AimingGridType);
        }
        else
        {
            CActing acting = null;
            if (ControlledThing.GetComponent<CActing>(out var component))
                acting = (CActing)component;

            if (!acting.IsActionReady)
            {
                QueuedAction = new UseWieldedThingAction();
                QueuedActionName = QueuedAction.ToString();
                return;
            }

            wieldedThing.Use(ControlledThing);
        }
    }

    public void UseWieldedThing(Direction direction)
    {
        CActing acting = null;
        if (ControlledThing.GetComponent<CActing>(out var component))
            acting = (CActing)component;

        if (!acting.IsActionReady)
        {
            QueuedAction = new UseWieldedThingDirectionAction(direction);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        ControlledThing.UseWieldedThing(direction);
    }

    public void UseWieldedThing(GridType gridType, IntVector targetGridPos)
    {
        CActing acting = null;
        if (ControlledThing.GetComponent<CActing>(out var component))
            acting = (CActing)component;

        if (!acting.IsActionReady)
        {
            QueuedAction = new UseWieldedThingTargetAction(gridType, targetGridPos);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        ControlledThing.UseWieldedThing(gridType, targetGridPos);
    }

    public void MoveThingTo(Thing thing, GridType targetGridType, IntVector targetGridPos, bool dontRequireAction = false, bool wieldIfPossible = false, bool playSfx = false)
    {
        if (IsAiming)
            StopAiming();

        CActing acting = null;
        if (ControlledThing.GetComponent<CActing>(out var component))
            acting = (CActing)component;

        if (!acting.IsActionReady && !dontRequireAction)
        {
            QueuedAction = new MoveThingAction(thing, targetGridType, targetGridPos, thing.ContainingGridType, thing.GridPos, wieldIfPossible, playSfx);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        var sourceGridType = thing.ContainingGridType;
        Sandbox.Diagnostics.Assert.True(sourceGridType != targetGridType);

        var owningPlayer = thing.ContainingGridManager?.OwningPlayer;

        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType: sourceGridType);
        RoguemojiGame.Instance.RefreshGridPanelClient(To.Single(this), gridType: targetGridType);

        if (targetGridType == GridType.Arena || sourceGridType == GridType.Arena)
        {
            RoguemojiGame.Instance.RefreshNearbyPanelClient(To.Single(this));
            RoguemojiGame.Instance.FlickerNearbyPanelCellsClient(To.Single(this));
        }

        thing.ContainingGridManager?.RemoveThing(thing);
        var targetGridManager = GetGridManager(targetGridType);

        Thing existingInvEquipItem = (targetGridType != GridType.Arena) ? targetGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault() : null;
        IntVector sourceGridPos = thing.GridPos;

        targetGridManager.AddThing(thing);
        thing.SetGridPos(targetGridPos);

        if (existingInvEquipItem != null)
        {
            if (sourceGridType == GridType.Equipment && targetGridType == GridType.Inventory && !existingInvEquipItem.HasFlag(ThingFlags.Equipment))
            {
                if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    SwapGridThingPos(existingInvEquipItem, GridType.Inventory, emptyGridPos);
                else
                    MoveThingTo(existingInvEquipItem, GridType.Arena, ControlledThing.GridPos, dontRequireAction: true, playSfx: true);
            }
            else
            {
                MoveThingTo(existingInvEquipItem, sourceGridType, sourceGridPos, dontRequireAction: true);
            }
        }

        if (sourceGridType == GridType.Equipment && owningPlayer != null)
            owningPlayer.ControlledThing.UnequipThing(thing);

        if (targetGridType == GridType.Arena)
        {
            if (thing == ControlledThing.WieldedThing)
                WieldThing(null, dontRequireAction: true);

            thing.CurrentLevelId = ControlledThing.CurrentLevelId;

            thing.ThingOwningThis = null;

            if (playSfx)
                thing.PlaySfx(SoundActionType.Drop);
        }

        if(targetGridType == GridType.Inventory || targetGridType == GridType.Equipment)
        {
            if (targetGridType == GridType.Inventory)
            {
                if (wieldIfPossible && ControlledThing.WieldedThing == null && !thing.HasFlag(ThingFlags.Equipment))
                    WieldThing(thing, dontRequireAction: true);

                thing.ThingOwningThis = ControlledThing;
            }

            if (targetGridType == GridType.Equipment)
            {
                targetGridManager.OwningPlayer.ControlledThing.EquipThing(thing);
                thing.ThingOwningThis = ControlledThing;
            }

            if(playSfx)
            {
                if(sourceGridType == GridType.Arena)
                    thing.PlaySfx(SoundActionType.PickUp);
                else
                    PlaySfxUI("plop");
            }
        }

        if (!dontRequireAction)
            acting.PerformedAction();
    }

    public void WieldThing(Thing thing, bool dontRequireAction = false)
    {
        if (ControlledThing.WieldedThing == thing)
            return;

        if (IsAiming)
            StopAiming();

        CActing acting = null;
        if (ControlledThing.GetComponent<CActing>(out var component))
            acting = (CActing)component;

        if (!acting.IsActionReady && !dontRequireAction)
        {
            QueuedAction = new WieldThingAction(thing);
            QueuedActionName = QueuedAction.ToString();
            return;
        }

        ControlledThing.WieldThing(thing);

        if(thing != null)
            thing.PlaySfx(SoundActionType.Wield);

        if (!dontRequireAction)
            acting.PerformedAction();
    }

    public void SwapGridThingPos(Thing thing, GridType gridType, IntVector targetGridPos)
    {
        if (gridType == GridType.Arena)
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
        if (ControlledThing == null)
            return;

        if (gridType == GridType.Arena)
        {
            var level = RoguemojiGame.Instance.GetLevel(ControlledThing.CurrentLevelId);

            if(!level.GridManager.IsGridPosInBounds(gridPos))
            {
                SelectThing(null);
                return;
            }

            var thing = level.GridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).Where(x => ControlledThing.CanPerceiveThing(x)).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (!visible && thing != null)
                return;

            if (!rightClick)
                SelectThing(thing, playSfx: true);
        }
        else if (gridType == GridType.Inventory)
        {
            var thing = InventoryGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (thing != null && (doubleClick || rightClick))
            {
                if (thing.HasFlag(ThingFlags.Equipment))
                    TryEquipThing(thing);
                else
                    WieldThing(thing);
            }
            else if (!rightClick)
            {
                if (thing != null && shift)
                    MoveThingTo(thing, GridType.Arena, ControlledThing.GridPos, playSfx: true);
                else
                    SelectThing(thing, playSfx: true);
            }
        }
        else if (gridType == GridType.Equipment)
        {
            var thing = EquipmentGridManager.GetThingsAt(gridPos).WithAll(ThingFlags.Selectable).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

            if (!rightClick)
            {
                if (thing != null && shift)
                    MoveThingTo(thing, GridType.Arena, ControlledThing.GridPos, playSfx: true);
                else
                    SelectThing(thing, playSfx: true);
            }
            else
            {
                if (thing != null && InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                    MoveThingTo(thing, GridType.Inventory, emptyGridPos, playSfx: true);
            }
        }
    }

    public void ClickedNothing()
    {
        SelectThing(null);
    }

    public void NearbyThingClicked(Thing thing, bool rightClick, bool shift, bool doubleClick)
    {
        if (shift || rightClick || doubleClick)
        {
            if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
                MoveThingTo(thing, GridType.Inventory, emptyGridPos, wieldIfPossible: true, playSfx: true);
        }
        else
        {
            SelectThing(thing, playSfx: true);
        }
    }

    public void InventoryThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos, bool draggedWieldedThing)
    {
        if (thing == null || !thing.IsValid)
            return;

        if (destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.Nearby || destinationPanelType == PanelType.None)
        {
            MoveThingTo(thing, GridType.Arena, ControlledThing.GridPos, playSfx: true);
        }
        else if (destinationPanelType == PanelType.InventoryGrid)
        {
            if (draggedWieldedThing)
            {
                if (thing.GridPos.Equals(targetGridPos))
                {
                    WieldThing(null);
                }
                else
                {
                    var targetThing = InventoryGridManager.GetThingsAt(targetGridPos).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
                    WieldThing(targetThing == null || targetThing.HasFlag(ThingFlags.Equipment) ? null : targetThing);
                    SwapGridThingPos(thing, GridType.Inventory, targetGridPos);
                    PlaySfxUI("plop");
                }
            }
            else
            {
                if (!thing.GridPos.Equals(targetGridPos))
                {
                    SwapGridThingPos(thing, GridType.Inventory, targetGridPos);
                    PlaySfxUI("plop");
                }
                else
                {
                    SelectThing(thing, playSfx: true);
                }
            }
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.HasFlag(ThingFlags.Equipment))
                return;

            MoveThingTo(thing, GridType.Equipment, targetGridPos, playSfx: true);
        }
        else if (destinationPanelType == PanelType.Wielding)
        {
            if (ControlledThing.WieldedThing == thing)
                SelectThing(thing, playSfx: true);
            else if (!thing.HasFlag(ThingFlags.Equipment))
                WieldThing(thing);
        }
        else if (destinationPanelType == PanelType.CharPortrait)
        {
            if (thing.HasFlag(ThingFlags.Equipment))
                TryEquipThing(thing);
            else
                WieldThing(thing);
        }
        else if (destinationPanelType == PanelType.Info)
        {
            SelectThing(thing, playSfx: true);
        }
    }

    public void EquipmentThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos)
    {
        if (thing == null || !thing.IsValid)
            return;

        if (destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.Nearby)// || destinationPanelType == PanelType.None)
        {
            MoveThingTo(thing, GridType.Arena, ControlledThing.GridPos, playSfx: true);
        }
        else if (destinationPanelType == PanelType.InventoryGrid)
        {
            MoveThingTo(thing, GridType.Inventory, targetGridPos, playSfx: true);
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.GridPos.Equals(targetGridPos))
            {
                SwapGridThingPos(thing, GridType.Equipment, targetGridPos);
                PlaySfxUI("plop");
            }
            else
            {
                SelectThing(thing, playSfx: true);
            }
        }
        else if (destinationPanelType == PanelType.Info)
        {
            SelectThing(thing, playSfx: true);
        }
    }

    public void NearbyThingDragged(Thing thing, PanelType destinationPanelType, IntVector targetGridPos)
    {
        if (thing == null || !thing.IsValid)
            return;

        // dont allow dragging nearby thing from different cells, or if the thing has been picked up by someone else
        if (!ControlledThing.GridPos.Equals(thing.GridPos) || thing.ContainingGridType == GridType.Inventory)
            return;

        if (destinationPanelType == PanelType.InventoryGrid)
        {
            MoveThingTo(thing, GridType.Inventory, targetGridPos, playSfx: true);
        }
        else if (destinationPanelType == PanelType.EquipmentGrid)
        {
            if (!thing.HasFlag(ThingFlags.Equipment))
                return;

            MoveThingTo(thing, GridType.Equipment, targetGridPos, playSfx: true);
        }
        else if (destinationPanelType == PanelType.Nearby)
        {
            SelectThing(thing, playSfx: true);
        }
        else if (destinationPanelType == PanelType.Wielding)
        {
            if (thing.HasFlag(ThingFlags.Equipment))
                return;

            if (InventoryGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
            {
                MoveThingTo(thing, GridType.Inventory, emptyGridPos, playSfx: true);
                WieldThing(thing, dontRequireAction: true);
            }
        }
        else if (destinationPanelType == PanelType.CharPortrait)
        {
            // todo
        }
        else if (destinationPanelType == PanelType.Info)
        {
            SelectThing(thing, playSfx: true);
        }
    }

    public void WieldingClicked(bool rightClick, bool shift)
    {
        if (ControlledThing.WieldedThing == null)
            return;

        if (rightClick)
            WieldThing(null);
        else if (shift)
            MoveThingTo(ControlledThing.WieldedThing, GridType.Arena, ControlledThing.GridPos, playSfx: true);
        else
            SelectThing(ControlledThing.WieldedThing, playSfx: true);
    }

    public void PlayerIconClicked(bool rightClick, bool shift)
    {
        SelectThing(ControlledThing, playSfx: true);
    }

    public void CharacterHotkeyPressed()
    {
        SelectThing(ControlledThing, playSfx: true);
    }

    public GridManager GetGridManager(GridType gridType)
    {
        switch (gridType)
        {
            case GridType.Arena:
                return ControlledThing.ContainingGridManager;
            case GridType.Inventory:
                return InventoryGridManager;
            case GridType.Equipment:
                return EquipmentGridManager;
        }

        return null;
    }

    public override void OnChangedStat(StatType statType, int changeCurrent, int changeMin, int changeMax)
    {
        if (statType == StatType.Sight)
        {
            RefreshVisibility();
        }
    }

    public void StartAimingThrow()
    {
        if (ControlledThing.WieldedThing == null)
            return;

        StartAiming(AimingSource.Throwing, AimingType.Direction, GridType.Arena);
        //RoguemojiGame.Instance.LogMessageClient(To.Single(this), "Press WASD to throw or F to cancel", playerNum: 0);
    }

    public void StartAiming(AimingSource aimingSource, AimingType aimingType, GridType aimingGridType)
    {
        if (QueuedAction != null)
        {
            QueuedAction = null;
            QueuedActionName = "";
        }

        IsAiming = true;
        AimingSource = aimingSource;
        AimingType = aimingType;
        AimingGridType = aimingGridType;

        if(aimingType == AimingType.Direction)
        {
            AimDirectionClient(To.Single(this));
        }
        else if(aimingSource == AimingSource.UsingWieldedItem && aimingType == AimingType.TargetCell)
        {
            if(ControlledThing.WieldedThing != null)
                AimTargetCellsClient(To.Single(this), ControlledThing.WieldedThing.NetworkIdent);
            else
                StopAiming();
        }
    }

    [ClientRpc]
    public void AimDirectionClient()
    {
        AimingCells.Clear();

        foreach (var dir in GridManager.GetCardinalDirections())
        {
            IntVector gridPos = ControlledThing.GridPos + GridManager.GetIntVectorForDirection(dir);
            AimingCells.Add(gridPos);
        }
    }

    public void RefreshWieldedThingTargetAiming()
    {
        if (ControlledThing.WieldedThing == null || !IsAiming || AimingSource != AimingSource.UsingWieldedItem || AimingType != AimingType.TargetCell)
            return;

        AimTargetCellsClient(To.Single(this), ControlledThing.WieldedThing.NetworkIdent);
    }

    [ClientRpc]
    public void AimTargetCellsClient(int networkIdent)
    {
        AimingCells.Clear();

        Thing usedThing = Entity.FindByIndex(networkIdent) as Thing;
        var aimingCells = usedThing.GetAimingTargetCellsClient();

        foreach (IntVector gridPos in aimingCells)
            AimingCells.Add(gridPos);
    }

    public void ConfirmAiming(Direction direction)
    {
        if (!IsAiming || AimingType != AimingType.Direction)
            return;

        StopAiming();

        if (IsConfused && Game.Random.Int(0, 2) == 0)
            direction = GridManager.GetRandomDirection(cardinalOnly: false);

        if (AimingSource == AimingSource.Throwing)
            ThrowWieldedThing(direction);
        else if (AimingSource == AimingSource.UsingWieldedItem)
            UseWieldedThing(direction);
    }

    public void ConfirmAiming(GridType gridType, IntVector gridPos)
    {
        if (!IsAiming)
            return;

        if(AimingType == AimingType.Direction)
        {
            var direction = GridManager.GetDirectionForIntVector(gridPos - ControlledThing.GridPos);
            ConfirmAiming(direction);
        }
        else if(AimingType == AimingType.TargetCell && AimingSource == AimingSource.UsingWieldedItem)
        {
            UseWieldedThing(gridType, gridPos);
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

        RefreshVisibility();
        ControlledThing.ContainingGridManager.PlayerChangedGridPos(this);

        if(IsAiming)
            RefreshWieldedThingTargetAiming();

        RoguemojiGame.Instance.FlickerNearbyPanelCellsClient();
    }

    public bool IsInInventory(Thing thing)
    {
        return thing.ContainingGridManager.GridType == GridType.Inventory && thing.ContainingGridManager.OwningPlayer == this;
    }

    public void IdentifyScroll(ScrollType scrollType)
    {
        if (!IdentifiedScrollTypes.Contains(scrollType))
        {
            IdentifiedScrollTypes.Add(scrollType);
            ControlledThing.AddFloater(Globals.Icon(IconType.Identified), 1f, new Vector2(-1f, -10f), new Vector2(-1f, -30f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.5f, scale: 0.8f, opacity: 0.66f);
            RoguemojiGame.Instance.LogMessageClient(To.Single(this), $"{Globals.Icon(IconType.Identified)}Identified 📜{RoguemojiGame.Instance.GetUnidentifiedScrollIcon(scrollType)} as {Scroll.GetDisplayName(scrollType)}{Scroll.GetChatDisplayIcons(scrollType)}", playerNum: 0);
        }
    }

    public bool IsScrollTypeIdentified(ScrollType scrollType)
    {
        return IdentifiedScrollTypes.Contains(scrollType);
    }

    public void IdentifyPotion(PotionType potionType)
    {
        if (!IdentifiedPotionTypes.Contains(potionType))
        {
            IdentifiedPotionTypes.Add(potionType);
            ControlledThing.AddFloater(Globals.Icon(IconType.Identified), 1f, new Vector2(0f, -10f), new Vector2(0, -30f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.5f, scale: 0.8f, opacity: 0.66f);
            RoguemojiGame.Instance.LogMessageClient(To.Single(this), $"{Globals.Icon(IconType.Identified)}Identified 🧉{RoguemojiGame.Instance.GetUnidentifiedPotionIcon(potionType)} as {Potion.GetDisplayName(potionType)}{Potion.GetChatDisplayIcons(potionType)}", playerNum: 0);
        }
    }

    public bool IsPotionTypeIdentified(PotionType potionType)
    {
        return IdentifiedPotionTypes.Contains(potionType);
    }

    public void DropAllItems()
    {
        int RANGE = 1;

        while(InventoryGridManager.Things.Count > 0)
        {
            if (Game.Random.Int(0, 1) == 0 && ControlledThing.ContainingGridManager.GetRandomEmptyGridPosWithinRange(ControlledThing.GridPos, out var emptyGridPos, RANGE, allowNonSolid: true))
                MoveThingTo(InventoryGridManager.Things[0], GridType.Arena, emptyGridPos, dontRequireAction: true);
            else
                MoveThingTo(InventoryGridManager.Things[0], GridType.Arena, ControlledThing.GridPos, dontRequireAction: true);
        }
            
        while (EquipmentGridManager.Things.Count > 0)
        {
            if (Game.Random.Int(0, 1) == 0 && ControlledThing.ContainingGridManager.GetRandomEmptyGridPosWithinRange(ControlledThing.GridPos, out var emptyGridPos, RANGE, allowNonSolid: true))
                MoveThingTo(EquipmentGridManager.Things[0], GridType.Arena, emptyGridPos, dontRequireAction: true);
            else
                MoveThingTo(EquipmentGridManager.Things[0], GridType.Arena, ControlledThing.GridPos, dontRequireAction: true);
        }

        DropWieldedItem();
    }

    public void PlaySfxUI(string name, float volume = 1f, float pitch = 1f)
    {
        //var sound = Sound.FromScreen(To.Single(Client), name, x: 0.5f, y: 0.5f);
        var sound = Sound.FromWorld(To.Single(Client), name, Vector3.Zero);
        sound.SetPitch(pitch);
        sound.SetVolume(volume);
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
    public bool PlaySfx { get; set; }

    public MoveThingAction(Thing thing, GridType targetGridType, IntVector targetGridPos, GridType sourceGridType, IntVector sourceGridPos, bool wieldIfPossible = false, bool playSfx = false)
    {
        Thing = thing;
        TargetGridType = targetGridType;
        TargetGridPos = targetGridPos;
        SourceGridType = sourceGridType;
        SourceGridPos = sourceGridPos;
        WieldIfPossible = wieldIfPossible;
        PlaySfx = playSfx;
    }

    public void Execute(RoguemojiPlayer player)
    {
        if (Thing.ContainingGridType != SourceGridType || !Thing.GridPos.Equals(SourceGridPos))
            return;

        player.MoveThingTo(Thing, TargetGridType, TargetGridPos, wieldIfPossible: WieldIfPossible, playSfx: PlaySfx);
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
        if (Thing == null || Thing != player.ControlledThing.WieldedThing)
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
    public GridType GridType { get; set; }
    public IntVector TargetGridPos { get; set; }

    public UseWieldedThingTargetAction(GridType gridType, IntVector targetGridPos)
    {
        GridType = gridType;
        TargetGridPos = targetGridPos;
    }

    public void Execute(RoguemojiPlayer player)
    {
        player.UseWieldedThing(GridType, TargetGridPos);
    }

    public override string ToString()
    {
        return $"UseWieldedThing {GridType}:{TargetGridPos}";
    }
}
