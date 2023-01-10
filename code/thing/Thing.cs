using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Roguemoji;

[Flags]
public enum ThingFlags
{
    None = 0,
    Solid = 1,
    Selectable = 2,
    Equipment = 4,
    Useable = 8,
    UseRequiresAiming = 16,
    AimTypeTargetCell = 32,
    CanUseThings = 64,
    CanBePickedUp = 128,
    Exclusive = 256,
    DoesntBumpThings = 512,
}

public enum FactionType { Neutral, Player, Enemy }

public class TattooData
{
    public string Icon { get; set; }
    public float Scale { get; set; }
    public Vector2 Offset { get; set; }
    public Vector2 OffsetWielded { get; set; }
    public Vector2 OffsetInfo { get; set; }
    public Vector2 OffsetCharWielded { get; set; }
    public Vector2 OffsetInfoWielded { get; set; }
}

public partial class Thing : Entity
{
    [Net] public IntVector GridPos { get; protected set; }
    [Net] public GridType ContainingGridType { get; set; }
    [Net] public GridManager ContainingGridManager { get; set; }

    [Net] public string DisplayIcon { get; protected set; }
    [Net] public string DisplayName { get; protected set; }
    [Net] public string Description { get; protected set; }
    [Net] public string Tooltip { get; protected set; }
    public virtual string ChatDisplayIcons => DisplayIcon;

    public bool ShouldUpdate { get; set; }
    [Net] public int PlayerNum { get; set; }

    [Net] public int IconDepth { get; set; }
    [Net] public int StackNum { get; set; }
    [Net] public float PathfindMovementCost { get; set; }

    public Vector2 Offset { get; set; }
    public float RotationDegrees { get; set; }
    public float IconScale { get; set; }
    public int CharSkip { get; set; } // Client-only

    [Net] public string DebugText { get; set; }

    [Net] public uint ThingId { get; private set; }

    [Net] public ThingFlags Flags { get; set; }
    public bool HasFlag(ThingFlags flag) => Flags.HasFlag(flag);

    [Net] public Thing WieldedThing { get; protected set; }
    [Net] public Thing ThingWieldingThis { get; protected set; }
    public Vector2 WieldedThingOffset { get; set; } // Client-only
    public int WieldedThingFontSize { get; set; } // Client-only
    public Vector2 InfoWieldedThingOffset { get; set; } // Client-only
    public int InfoWieldedThingFontSize { get; set; } // Client-only

    [Net] public int SightBlockAmount { get; set; }

    [Net] public IList<Thing> EquippedThings { get; private set; }

    [Net] public float ActionRechargePercent { get; set; }

    [Net] public LevelId CurrentLevelId { get; set; }

    public bool HasTattoo { get; set; } // Client-only
    public TattooData TattooData { get; set; } // Client-only

    [Net] public bool IsRemoved { get; set; }
    [Net] public bool IsOnCooldown { get; set; }
    [Net] public float CooldownProgressPercent { get; set; }

    [Net] public float CooldownTimer { get; set; }
    [Net] public float CooldownDuration { get; set; }

    [Net] public float StaminaTimer { get; set; }
    [Net] public float StaminaDelay { get; set; }

    [Net] public FactionType Faction { get; set; }

    [Net] public bool DontRender { get; set; } // Client-only

    // thing is entering/exiting a level and should not be able to act or be interacted with
    public bool IsInTransit { get; set; }

    public virtual string AbilityName => "Ability";

    public Thing()
    {
        ShouldUpdate = false;
        DisplayIcon = ".";
        DisplayName = Name;
        Tooltip = "";
        IconDepth = 0;
        IconScale = 1f;
        ThingId = RoguemojiGame.ThingId++;
        IsRemoved = false;
        IsOnCooldown = false;
        IsInTransit = false;
    }

    // Server only
    public override void Spawn()
    {
        base.Spawn();

        Transmit = TransmitType.Always;
    }

    public virtual void Update(float dt)
    {
        //DebugText = $"{GetStatClamped(StatType.Health)}";

        //DebugText = "Server Components (" + Components.Count + "):\n";
        for (int i = ThingComponents.Count - 1; i >= 0; i--)
        {
            KeyValuePair<TypeDescription, ThingComponent> pair = ThingComponents.ElementAt(i);

            var component = pair.Value;
            if (component.ShouldUpdate)
                component.Update(dt);

            //DebugText += component.GetType().Name + "\n";
        }

        if (IsOnCooldown)
            HandleCooldown(dt);

        if (HasStat(StatType.Energy) && HasStat(StatType.Stamina))
            HandleStamina(dt);

        // the wielded thing of NPCs has no ContainingGridManager, so must be updated manually (note: currently, if the wielded has ShouldUpdate=false, they will still not be getting updated)
        if(WieldedThing != null && WieldedThing.ShouldUpdate && WieldedThing.ContainingGridType == GridType.None)
            WieldedThing.Update(dt);
    }

    [Event.Tick.Client]
    public virtual void ClientTick()
    {
        float dt = Time.Delta;

        foreach (KeyValuePair<TypeDescription, ThingComponent> pair in ThingComponents)
        {
            var component = pair.Value;
            if (component.ShouldUpdate)
                component.Update(dt);
        }

        if (!string.IsNullOrEmpty(DebugText))
            DrawDebugText(DebugText);

        //DrawDebugText(ContainingGridManager?.Name.ToString() ?? "null");
        //DrawDebugText(Flags.ToString());

        //if(HasStat(StatType.Health))
        //    DrawDebugText($"{GetStat(StatType.Health).CurrentValue}");
    }

    public virtual bool TryMove(Direction direction, bool shouldAnimate = true)
    {
        Sandbox.Diagnostics.Assert.True(ContainingGridType != GridType.None);

        if(direction == Direction.None)
            return true;

        IntVector vec = GridManager.GetIntVectorForDirection(direction);
        IntVector newGridPos = GridPos + vec;

        if (!ContainingGridManager.IsGridPosInBounds(newGridPos))
        {
            VfxNudge(direction, 0.1f, 10f);
            OnBumpedOutOfBounds(direction);
            return false;
        }

        if(!HasFlag(ThingFlags.DoesntBumpThings))
        {
            Thing other = ContainingGridManager.GetThingsAt(newGridPos).WithAll(ThingFlags.Solid).Where(x => !x.IsInTransit).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
            if (other != null)
            {
                BumpInto(other, direction);
                //RoguemojiGame.Instance.LogMessage(DisplayIcon + "(" + DisplayName + ") bumped into " + other.DisplayIcon + "(" + other.DisplayName + ")", PlayerNum);

                return false;
            }
        }

        SetGridPos(newGridPos);

        if (shouldAnimate)
            VfxSlide(direction, 0.2f, 40f);

        return true;
    }

    public virtual void BumpInto(Thing target, Direction direction)
    {
        VfxNudge(direction, 0.1f, 10f);

        if(WieldedThing != null)
        {
            WieldedThing.HitOther(target, direction);
        }
        else
        {
            HitOther(target, direction);
        }

        OnBumpedIntoThing(target);
        target.OnBumpedIntoBy(this);
    }

    public virtual bool InteractWith(Thing target)
    {
        return false;
    }

    public virtual bool BeInteractedWith(Thing wieldedThing)
    {
        return false;
    }

    public virtual void HitOther(Thing target, Direction direction)
    {
        // todo: a way to force-feed food to other units
        //if (shouldUse && HasFlag(ThingFlags.Useable) && target.CanUseThing(this))
        //    Use(target);

        target.VfxShake(0.2f, 4f);

        if (target.HasStat(StatType.Health))
        {
            var damagingThing = ThingWieldingThis != null ? ThingWieldingThis : this;
            target.TakeDamage(damagingThing);
        }
    }

    public virtual void TakeDamage(Thing source)
    {
        if (!HasStat(StatType.Health))
            return;

        int amount = source.GetStatClamped(StatType.Attack);
        TakeDamage(amount);
    }

    public virtual void TakeDamage(int amount)
    {
        if (!HasStat(StatType.Health))
            return;

        if (amount > 0)
        {
            AdjustStat(StatType.Health, -amount);

            var startOffset = new Vector2(Game.Random.Float(-5f, 4f), Game.Random.Float(-5f, 4f));
            var endOffset = new Vector2(Game.Random.Float(-5f, 4f), Game.Random.Float(-5f, 4f));
            RoguemojiGame.Instance.AddFloater("💥", GridPos, 0.45f, CurrentLevelId, startOffset, endOffset, "", requireSight: true, EasingType.SineIn, 0.025f, parent: this);

            RoguemojiGame.Instance.AddFloater("💔", GridPos, 1.2f, CurrentLevelId, new Vector2(0f, 1f), new Vector2(0f, -6f), $"-{amount}", requireSight: true, EasingType.SineOut, 0.25f, parent: this);

            if (GetStatClamped(StatType.Health) <= 0)
            {
                RoguemojiGame.Instance.AddFloater("☠️", GridPos, 1.5f, CurrentLevelId, new Vector2(0f, 4f), new Vector2(0f, -7f), "", requireSight: true, EasingType.SineOut, 1f, parent: this);
                Destroy();
            }
        }
    }

    public virtual void UseWieldedThing()
    {
        if (WieldedThing == null)
            return;

        // todo: AI needs to choose direction/target cell for aimed useables
        WieldedThing.Use(this);
    }

    public virtual void UseWieldedThing(Direction direction)
    {
        if (WieldedThing == null)
            return;

        WieldedThing.Use(this, direction);
    }

    public virtual void UseWieldedThing(IntVector targetGridPos)
    {
        if (WieldedThing == null)
            return;

        WieldedThing.Use(this, targetGridPos);
    }

    // Override and return false when user doesn't have required stats (IsOnCooldown already handled elsewhere).
    public virtual bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        return true;
    }

    public virtual void Use(Thing user) 
    {
        PerformedAction(user);
    }

    public virtual void Use(Thing user, Direction direction) 
    {
        PerformedAction(user);
    }

    public virtual void Use(Thing user, IntVector targetGridPos) 
    {
        PerformedAction(user);
    }

    public virtual void PerformedAction(Thing user)
    {
        if (user.GetComponent<CActing>(out var acting))
            ((CActing)acting).PerformedAction();
    }

    public virtual void Destroy()
    {
        if(ThingWieldingThis != null)
            ThingWieldingThis.WieldThing(null);

        if (WieldedThing != null)
            WieldThing(null);

        OnDestroyed();

        Remove();
    }

    public virtual void SetGridPos(IntVector gridPos)
    {
        Sandbox.Diagnostics.Assert.True(ContainingGridManager.IsGridPosInBounds(gridPos));

        //if ( GridPos.Equals( gridPos ) && !forceRefresh )
        //	return;

        if (this == null || !IsValid || IsRemoved)
            return;

        ContainingGridManager.SetGridPos(this, gridPos);
        GridPos = gridPos;

        if (ContainingGridManager.GridType == GridType.Inventory || ContainingGridManager.GridType == GridType.Equipment)
            RefreshGridPanelClient(To.Single(ContainingGridManager.OwningPlayer));
        else
            RefreshGridPanelClient();

        OnChangedGridPos();

        var existingThings = ContainingGridManager.GetThingsAt(gridPos);
        for (int i = existingThings.Count() - 1; i >= 0; i--)
        {
            var thing = existingThings.ElementAt(i);
            if (thing == this)
                continue;

            OnMovedOntoThing(thing);
            thing.OnMovedOntoBy(this);
        }
    }

    private void Remove()
    {
        if (IsRemoved)
            return;

        IsRemoved = true;

        if (ContainingGridType != GridType.None)
            ContainingGridManager.RemoveThing(this);

        Delete();
    }

    [ClientRpc]
    public void RefreshGridPanelClient()
    {
        if (Hud.Instance == null)
            return;

        GridPanel panel = Hud.Instance.GetGridPanel(ContainingGridType);
        if (panel == null)
            return;

        panel.StateHasChanged();
    }

    [ClientRpc]
    public void SetTransformClient(float offsetX = 0f, float offsetY = 0f, float degrees = 0f, float scale = 1f)
    {
        Offset = new Vector2(offsetX, offsetY);
        RotationDegrees = degrees;
        IconScale = scale;
    }

    public void SetOffset(Vector2 offset)
    {
        Offset = offset;
    }

    public void SetRotation(float rotationDegrees)
    {
        RotationDegrees = rotationDegrees;
    }

    public void SetScale(float scale)
    {
        IconScale = scale;
    }

    public void DrawDebugText(string text, Color color, int line = 0, float time = 0f)
    {
        if (Game.IsServer)
        {
            DebugOverlay.ScreenText(text, new Vector2(20f, 20f), 0, color, time);
        }
        else
        {
            var player = RoguemojiGame.Instance.LocalPlayer;
            var offsetGridPos = GridPos - player.CameraGridOffset;

            var panel = Hud.Instance.GetGridPanel(ContainingGridType);
            if (panel != null)
                DebugOverlay.ScreenText(text, panel.GetCellPos(offsetGridPos), line, color, time);
        }
    }

    public void DrawDebugText(string text)
    {
        DrawDebugText(text, new Color(1f, 1f, 1f, 0.5f));
    }

    public void SetIcon(string icon)
    {
        DisplayIcon = icon;
        //GridManager.RefreshGridPos(GridPos);
    }

    

    public override int GetHashCode()
    {
        return HashCode.Combine(DisplayIcon, WieldedThing?.ThingId ?? 0, PlayerNum + ThingId, RotationDegrees, IconScale, IconDepth, Flags);
        //return HashCode.Combine((DisplayIcon + ThingId.ToString()), PlayerNum, Offset, RotationDegrees, IconScale, IconDepth, Flags);
    }

    public int GetInfoDisplayHash()
    {
        // todo: check all stats
        return HashCode.Combine(NetworkIdent, DisplayIcon, WieldedThing?.DisplayIcon ?? "", GetStatClamped(StatType.Health), GetStatMax(StatType.Health), Flags);
    }

    public int GetNearbyCellHash()
    {
        return HashCode.Combine(DisplayIcon, PlayerNum, IconDepth, Flags, NetworkIdent, ThingId);
    }

    public int GetZPos()
    {
        return (IconDepth * 100) + StackNum;
    }

    public virtual void WieldThing(Thing thing)
    {
        if (thing == WieldedThing)
            return;

        if (WieldedThing != null)
        {
            OnNoLongerWieldingThing(thing);
            WieldedThing.OnNoLongerWieldedBy(this);
        }

        WieldedThing = thing;

        OnWieldThing(thing); // thing may be null here

        if (WieldedThing != null)
        {
            thing.OnWieldedBy(this);
        }
    }

    public void WieldAndRemoveFromGrid(Thing thing)
    {
        WieldThing(thing);

        if (thing.ContainingGridType != GridType.None)
            thing.ContainingGridManager.RemoveThing(thing);
    }

    /// <summary> For players, use MoveThingTo to equip things, and this will be called automatically. </summary>
    public void EquipThing(Thing thing)
    {
        if (EquippedThings == null)
            EquippedThings = new List<Thing>();

        RoguemojiGame.Instance.AddFloater(thing.DisplayIcon, GridPos, 0.6f, CurrentLevelId, new Vector2(0f, 0f), new Vector2(0f, -7f), "", requireSight: true, EasingType.SineOut, 0.05f, parent: this);

        EquippedThings.Add(thing);

        OnEquipThing(thing);
        thing.OnEquippedTo(this);
    }

    /// <summary> For players, use MoveThingTo to unequip things, and this will be called automatically. </summary>
    public void UnequipThing(Thing thing)
    {
        RoguemojiGame.Instance.AddFloater(thing.DisplayIcon, GridPos, 0.6f, CurrentLevelId, new Vector2(0f, -8f), new Vector2(0f, 0f), "", requireSight: true, EasingType.SineOut, 0.05f, parent: this);

        EquippedThings.Remove(thing);

        OnUnequipThing(thing);
        thing.OnUnequippedFrom(this);
    }

    public bool HasEquipmentType(TypeDescription type)
    {
        if (EquippedThings == null)
            return false;

        foreach (var thing in EquippedThings)
        {
            if (type == TypeLibrary.GetType(thing.GetType()))
                return true;
        }

        return false;
    }

    public virtual void Restart()
    {
        CurrentLevelId = LevelId.None;
        IsRemoved = false;
        EquippedThings.Clear();
        WieldedThing = null;
        ThingWieldingThis = null;
        IsOnCooldown = false;
        StaminaTimer = 0f;
        StaminaDelay = 0f;
        StatHash = 0;
        DontRender = false;
    }

    public virtual HashSet<IntVector> GetAimingTargetCellsClient() { return null; }
    public virtual bool IsPotentialAimingTargetCell(IntVector gridPos) { return false; }

    public void SetTattoo(string icon, float scale, Vector2 offset, Vector2 offsetWielded, Vector2 offsetInfo, Vector2 offsetCharWielded, Vector2 offsetInfoWielded)
    {
        SetTattooClient(icon, scale, offset, offsetWielded, offsetInfo, offsetCharWielded, offsetInfoWielded);
    }

    [ClientRpc]
    public void SetTattooClient(string icon, float scale, Vector2 offset, Vector2 offsetWielded, Vector2 offsetInfo, Vector2 offsetCharWielded, Vector2 offsetInfoWielded)
    {
        HasTattoo = true;
        TattooData = new TattooData()
        {
            Icon = icon,
            Scale = scale,
            Offset = offset,
            OffsetWielded = offsetWielded,
            OffsetInfo = offsetInfo,
            OffsetCharWielded = offsetCharWielded,
            OffsetInfoWielded = offsetInfoWielded,
        };
    }

    public void RemoveTattoo()
    {
        HasTattoo = false;
    }

    public void StartCooldown(float time)
    {
        CooldownDuration = time;
        CooldownTimer = time;
        IsOnCooldown = true;
        CooldownProgressPercent = 0f;
        OnCooldownStart();
    }

    void HandleCooldown(float dt)
    {
        CooldownTimer -= dt;
        if (CooldownTimer < 0f)
            FinishCooldown();
        else
            CooldownProgressPercent = Utils.Map(CooldownTimer, CooldownDuration, 0f, 0f, 1f);
    }

    public void FinishCooldown()
    {
        IsOnCooldown = false;
        OnCooldownFinish();
    }

    void HandleStamina(float dt)
    {
        int energy = GetStatClamped(StatType.Energy);
        int energyMax = GetStatMax(StatType.Energy);

        if(energy < energyMax)
        {
            StaminaTimer -= dt;
            if (StaminaTimer < 0f)
            {
                StaminaTimer += StaminaDelay;
                AdjustStat(StatType.Energy, 1);
            }
        }
    }
}
