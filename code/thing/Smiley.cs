using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Smiley : Thing
{
    public CActing Acting { get; private set; }
    public CIconPriority IconPriority { get; private set; }

    public Smiley()
	{
        ShouldUpdate = true;
		DisplayName = "Smiley";
		Tooltip = "";
        PathfindMovementCost = 10f;
        Flammability = 13;

        if (Game.IsServer)
        {
            SetStartingValues();
        }
        else
        {
            WieldedThingOffset = new Vector2(20f, 17f);
            WieldedThingFontSize = 18;
            InfoWieldedThingOffset = new Vector2(38f, 38f);
            InfoWieldedThingFontSize = 32;
        }
	}

    public override void Spawn()
    {
        base.Spawn();

        Acting = AddComponent<CActing>();
        IconPriority = AddComponent<CIconPriority>();
        IconPriority.SetDefaultIcon("😀");
    }

    //[Event.Tick.Client]
    //public virtual void ClientTick()
    //{
    //    base.ClientTick();

    //    DrawDebugText($"{GridPos}");
    //}

    void SetStartingValues()
    {
        DisplayIcon = "😀";
        IconDepth = (int)IconDepthLevel.Player;
        Flags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CanWieldThings;
        //ActionDelay = TimeSinceAction = 0.5f;
        //IsActionReady = true;
        Faction = FactionType.Player;
        IsInTransit = false;
        FloaterNum = 0;

        ClearStats();
        InitStat(StatType.Health, 10, 0, 10);
        InitStat(StatType.Energy, 0, 0, 0);
        InitStat(StatType.Mana, 0, 0, 0);
        InitStat(StatType.Strength, 1);
        InitStat(StatType.Speed, 13);
        InitStat(StatType.Intelligence, 4);
        InitStat(StatType.Stamina, 4);
        InitStat(StatType.Stealth, 0, -999, 999);
        InitStat(StatType.Charisma, 3);
        InitStat(StatType.Sight, 7, min: 0); // setting this will RefreshVisibility for the player
        InitStat(StatType.Hearing, 5);
        InitStat(StatType.SightBlockAmount, 10);
        //InitStat(StatType.Smell, 1);
        FinishInitStats();

        StaminaTimer = StaminaDelay;

        ClearTraits();
    }


    public override bool TryMove(Direction direction, out bool switchedLevel, bool shouldAnimate = true, bool shouldQueueAction = false, bool dontRequireAction = false)
    {
        switchedLevel = false;

        if (IsInTransit)
            return false;

        var success = base.TryMove(direction, out switchedLevel, shouldAnimate, shouldQueueAction: false, dontRequireAction);
		if (success)
		{
            if(Game.Random.Int(0, 5) == 0)
                IconPriority.AddIconPriority(Utils.GetRandomIcon("😄", "🙂"), (int)PlayerIconPriority.Move, 1.0f);
        }
        else 
		{
            IconPriority.AddIconPriority("😠", (int)PlayerIconPriority.Attack, 0.4f);
        }

        if(!dontRequireAction)
            Acting.PerformedAction();

		return success;
	}

    public override void BumpInto(Thing other, Direction direction)
    {
        base.BumpInto(other, direction);

        if(other is Door)
        {
            if (CurrentLevelId == LevelId.Forest2)
                RoguemojiGame.Instance.ChangeThingLevel(this, LevelId.Forest1);
            else if (CurrentLevelId == LevelId.Forest3)
                RoguemojiGame.Instance.ChangeThingLevel(this, LevelId.Forest2);
        }
    }

    public override void TakeDamageFrom(Thing thing)
    {
        base.TakeDamageFrom(thing);

        IconPriority.AddIconPriority(Utils.GetRandomIcon("😲", "😲", "😧", "😨") , (int)PlayerIconPriority.TakeDamage, 1.0f);
    }

    //public override void Destroy()
    //{
    //    IconPriority.AddIconPriority("💀", (int)PlayerIconPriority.Dead);

    //    OnDied();
    //}

    //public void PickUpTopItem()
    //{
    //    var thing = ContainingGridManager.GetThingsAt(GridPos).WithAll(ThingFlags.CanBePickedUp).WithNone(ThingFlags.Solid).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
    //    TryPickUp(thing);
    //}

    public bool TryPickUp(Thing thing, bool dontRequireAction = true)
    {
        if (thing == null)
            return false;

        return false;
    }

    //public void ThrowWieldedThing(Direction direction)
    //{
    //    if (WieldedThing == null || direction == Direction.None)
    //        return;

    //    var projectile = WieldedThing.AddComponent<CProjectile>();
    //    projectile.Direction = direction;
    //    projectile.MoveDelay = 0.1f;
    //    projectile.TotalDistance = 5;
    //    projectile.Thrower = this;

    //    MoveThingTo(WieldedThing, GridType.Arena, GridPos);
    //}

    //public void DropWieldedItem()
    //{
    //    if (WieldedThing != null)
    //        MoveThingTo(WieldedThing, GridType.Arena, GridPos);
    //}

    //void TryEquipThing(Thing thing)
    //{
    //    //if (EquipmentGridManager.GetFirstEmptyGridPos(out var emptyGridPos))
    //    //    MoveThingTo(thing, GridType.Equipment, emptyGridPos);
    //}

    public override void UseWieldedThing()
    {
        if (WieldedThing == null)
            return;

        if (!WieldedThing.HasFlag(ThingFlags.Useable))
            return;

        if (WieldedThing.IsOnCooldown)
            return;
            
        if(!WieldedThing.CanBeUsedBy(this, shouldLogMessage: true))
            return;

        if (WieldedThing.HasFlag(ThingFlags.UseRequiresAiming))
        {
            //AimingType aimingType = WieldedThing.HasFlag(ThingFlags.AimTypeTargetCell) ? AimingType.TargetCell : AimingType.Direction;
            //StartAiming(AimingSource.UsingWieldedItem, aimingType, WieldedThing.AimingGridType);
        }
        else
        {
            WieldedThing.Use(this);
        }
    }

    public override void UseWieldedThing(Direction direction)
    {
        base.UseWieldedThing(direction);
    }

    public override void UseWieldedThing(GridType gridType, IntVector targetGridPos)
    {
        base.UseWieldedThing(gridType, targetGridPos);
    }

    public void MoveThingTo(Thing thing, GridType targetGridType, IntVector targetGridPos, bool dontRequireAction = false, bool wieldIfPossible = false)
    {
        
    }

    //public void WieldThing(Thing thing, bool dontRequireAction = false)
    //{
    //    base.WieldThing(thing);

    //    if (!dontRequireAction)
    //        Acting.PerformedAction();
    //}

    void FinishInitStats()
    {
        var mana = GetStat(StatType.Mana);
        if (mana != null)
            mana.CurrentValue = mana.MaxValue;

        var energy = GetStat(StatType.Energy);
        if (energy != null)
            energy.CurrentValue = energy.MaxValue;
    }

    //public void StartAimingThrow()
    //{
    //    if (WieldedThing == null)
    //        return;

    //    //StartAiming(AimingSource.Throwing, AimingType.Direction, GridType.Arena);
    //    //RoguemojiGame.Instance.LogMessageClient(To.Single(this), "Press WASD to throw or F to cancel", playerNum: 0);
    //}

    //public void ConfirmAiming(Direction direction)
    //{
        
    //}

    //public void ConfirmAiming(GridType gridType, IntVector gridPos)
    //{

    //}

    //public bool IsInInventory(Thing thing)
    //{
    //    //return thing.ContainingGridManager.GridType == GridType.Inventory && thing.ContainingGridManager.OwningPlayer == this;
    //    return false;
    //}
}
