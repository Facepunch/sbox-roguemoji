﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

[Flags]
public enum ThingFlags
{
    None = 0,
    Solid = 1,
    Selectable = 2,
	Equipment = 4,
	Useable = 8,
	Food = 16,
	Animal = 32,
}

public partial class Thing : Entity
{
	[Net] public IntVector GridPos { get; protected set; }
	[Net] public GridManager ContainingGridManager { get; set; }

    [Net] public string DisplayIcon { get; protected set; }
	[Net] public string DisplayName { get; protected set; }
    [Net] public string Description { get; protected set; }
    [Net] public string Tooltip { get; protected set; }

	public bool ShouldUpdate { get; set; }
	public bool DoneFirstUpdate { get; protected set; }

	[Net] public int PlayerNum { get; set; }

	[Net] public int IconDepth { get; set; }
    public bool ShouldLogBehaviour { get; set; }
	[Net] public int StackNum { get; set; }
    [Net] public float PathfindMovementCost { get; set; }

    public Vector2 Offset { get; set; }
    public Vector2 TargetOffset { get; set; }
    public float RotationDegrees { get; set; }
	public float IconScale { get; set; }
    public int CharSkip { get; set; } // Client-only

	[Net] public string DebugText { get; set; }

    [Net] public int ThingId { get; private set; }

    public Dictionary<TypeDescription, ThingComponent> ThingComponents = new Dictionary<TypeDescription, ThingComponent>();

	[Net] public ThingFlags Flags { get; set; }

    [Net] public Thing WieldedThing { get; protected set; }
    [Net] public Thing ThingWieldingThis { get; protected set; }

    [Net] public int SightBlockAmount { get; set; }

	[Net] public IList<Thing> EquippedThings { get; private set; }

	[Net] public float ActionRechargePercent { get; set; }

    public Thing()
	{
        ShouldUpdate = true;
		DisplayIcon = ".";
		DisplayName = Name;
		Tooltip = "";
		IconDepth = 0;
		ShouldLogBehaviour = false;
		IconScale = 1f;
		ThingId = RoguemojiGame.ThingId++;
    }

	// Server only
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
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

		if(!string.IsNullOrEmpty(DebugText))
			DrawDebugText(DebugText);

        Offset = Utils.DynamicEaseTo(Offset, TargetOffset, 0.6f, dt);

        //DrawDebugText(ContainingGridManager?.Name.ToString() ?? "null");
        //DrawDebugText(Flags.ToString());
    }

	public virtual void Update(float dt)
	{
        //DebugText = "Server Components (" + Components.Count + "):\n";
        foreach (KeyValuePair<TypeDescription, ThingComponent> pair in ThingComponents)
        {
			var component = pair.Value;
			if (component.ShouldUpdate)
				component.Update(dt);

			//DebugText += component.GetType().Name + "\n";
		}

		//DrawDebugText(Flags.ToString());
	}

	public virtual void FirstUpdate()
	{
		//SetGridPos(GridPos);
		DoneFirstUpdate = true;
    }

	public virtual bool TryMove(Direction direction, bool shouldAnimate = true)
	{
		Sandbox.Diagnostics.Assert.True(direction != Direction.None);

		IntVector vec = GridManager.GetIntVectorForDirection(direction);
		IntVector newGridPos = GridPos + vec;

		if ( !ContainingGridManager.IsGridPosInBounds( newGridPos ) )
			return false;

        Thing other = ContainingGridManager.GetThingsAt(newGridPos).WithAll(ThingFlags.Solid).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        if (other != null)
		{
            BumpInto(other, direction);
            //RoguemojiGame.Instance.LogMessage(DisplayIcon + "(" + DisplayName + ") bumped into " + other.DisplayIcon + "(" + other.DisplayName + ")", PlayerNum);

			return false;
		}

		SetGridPos(newGridPos);

		if(shouldAnimate)
			VfxSlide(direction, 0.2f, 40f);

		return true;
	}

    public virtual void BumpInto(Thing target, Direction direction)
	{
        VfxNudge(direction, 0.1f, 10f);

		var hittingThing = WieldedThing != null ? WieldedThing : this;
        hittingThing.HitOther(target, direction);
    }

	public virtual void HitOther(Thing target, Direction direction)
	{
		if (Flags.HasFlag(ThingFlags.Useable))
			Use(target);
		else
			DamageOther(target, direction);
    }

	public virtual void DamageOther(Thing target, Direction direction)
	{
        target.VfxShake(0.2f, 4f);

        if (target.HasStat(StatType.Health))
        {
            var explosion = target.ContainingGridManager.SpawnThing<Explosion>(target.GridPos);
            explosion.VfxShake(0.15f, 6f);
            explosion.VfxScale(0.15f, 0.5f, 1f);

            var damagingThing = ThingWieldingThis != null ? ThingWieldingThis : this;
            target.TakeDamage(damagingThing);

            int amount = damagingThing.GetStatClamped(StatType.Attack);
            RoguemojiGame.Instance.LogMessage($"{damagingThing.DisplayIcon}{damagingThing.DisplayName} attacked {target.DisplayIcon}{target.DisplayName} for {amount}⚔️!", damagingThing.PlayerNum);
        }
    }

	public virtual void TakeDamage(Thing source)
	{
		if (!HasStat(StatType.Health))
			return;

		int amount = source.GetStatClamped(StatType.Attack);
		AdjustStat(StatType.Health, -amount);

		TakeDamageClient(amount, source.NetworkIdent);

		if(GetStatClamped(StatType.Health) <= 0)
		{
			TakeDamageLethalClient(amount, source.NetworkIdent);
            Destroy();
        }
			
	}

	[ClientRpc]
	public virtual void TakeDamageClient(int amount, int sourceNetworkIdent)
	{
        Hud.Instance.AddFloater("💔", GridPos, 1.2f, $"-{amount}", requireSight: true, 1f, -6f, EasingType.SineOut, 0.25f, parent: this);
    }

    [ClientRpc]
    public virtual void TakeDamageLethalClient(int amount, int sourceNetworkIdent)
    {
        Hud.Instance.AddFloater("☠️", GridPos, 1.5f, "", requireSight: true, 4f, -7f, EasingType.SineOut, 1f, parent: this);
    }

    public virtual void UseWieldedThing()
	{
		if (WieldedThing == null)
			return;

		WieldedThing.Use(this);
	}

	public virtual void Use(Thing target)
	{

	}

	public virtual void Destroy()
	{
		Remove();
	}

	public virtual void SetGridPos( IntVector gridPos )
	{
		//if ( GridPos.Equals( gridPos ) && !forceRefresh )
		//	return;

		ContainingGridManager.SetGridPos( this, gridPos );
		GridPos = gridPos;

		if(ContainingGridManager.GridType == GridType.Inventory || ContainingGridManager.GridType == GridType.Equipment)
            RefreshGridPanelClient(To.Single(ContainingGridManager.OwningPlayer));
		else
			RefreshGridPanelClient();

		//if (ShouldLogBehaviour)
  //      {
		//	if(Flags.HasFlag(ThingFlags.InInventory))
		//		RoguemojiGame.Instance.LogMessage(DisplayIcon + DisplayName + " moved to (" + gridPos.x + ", " + gridPos.y + ") in " + InventoryPlayer.DisplayName + "'s inventory.", PlayerNum);
		//	else
		//		RoguemojiGame.Instance.LogMessage(DisplayIcon + DisplayName + " moved to (" + gridPos.x + ", " + gridPos.y + ").", PlayerNum);
		//}
	}

	public void Remove()
	{
		//if ( ShouldLogBehaviour )
  //      {
		//	if (Flags.HasFlag(ThingFlags.InInventory))
		//		RoguemojiGame.Instance.LogMessage(DisplayIcon + DisplayName + " removed from " + InventoryPlayer.DisplayName + "'s inventory.", PlayerNum);
		//	else
		//		RoguemojiGame.Instance.LogMessage(DisplayIcon + DisplayName + " removed.", PlayerNum);
		//}
			
		ContainingGridManager.RemoveThing( this );
		Delete();
	}

	[ClientRpc]
    public void RefreshGridPanelClient()
    {
        if (Hud.Instance == null)
            return;

		GridPanel panel = Hud.Instance.GetGridPanel(ContainingGridManager.GridType);
        if (panel == null)
            return;

		panel.StateHasChanged();
    }

    public void SetOffset(Vector2 offset)
    {
		TargetOffset = offset;
    }

	public void SetRotation(float rotationDegrees)
	{
		RotationDegrees = rotationDegrees;
	}

	public void SetScale(float scale)
	{
		IconScale = scale;
	}

	public ThingComponent AddThingComponent(TypeDescription type)
	{
		if(type == null)
		{
			Log.Info("type is null!");
			return null;
		}

		if(ThingComponents.ContainsKey(type))
        {
			var component = ThingComponents[type];
			component.ReInitialize();
			return component;
        }
		else
        {
			var component = type.Create<ThingComponent>();
			component.Init(this);
			ThingComponents.Add(type, component);
			return component;
		}
	}

	public T AddThingComponent<T>() where T : ThingComponent
	{
		return AddThingComponent(TypeLibrary.GetType(typeof(T))) as T;
	}

	public void RemoveComponent(TypeDescription type)
    {
		if(ThingComponents.ContainsKey(type))
        {
			var component = ThingComponents[type];
			component.OnRemove();
			ThingComponents.Remove(type);
        }
    }

	public bool GetComponent(TypeDescription type, out ThingComponent component)
	{
		if (ThingComponents.ContainsKey(type))
		{
			component = ThingComponents[type];
			return true;
		}

		component = null;
		return false;
	}

	public void ForEachComponent(Action<ThingComponent> action)
	{
		foreach (var (_, component) in ThingComponents)
		{
			action(component);
		}
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
            
			var panel = Hud.Instance.GetGridPanel(ContainingGridManager.GridType);
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

	[ClientRpc]
	public void VfxNudge(Direction direction, float lifetime, float distance)
	{
		var nudge = AddThingComponent<VfxNudge>();
        nudge.Direction = direction;
        nudge.Lifetime = lifetime;
        nudge.Distance = distance;
    }

	[ClientRpc]
	public void VfxSlide(Direction direction, float lifetime, float distance)
    {
		var slide = AddThingComponent<VfxSlide>();
		slide.Direction = direction;
		slide.Lifetime = lifetime;
		slide.Distance = distance;
	}

	[ClientRpc]
	public void VfxShake(float lifetime, float distance)
	{
		var shake = AddThingComponent<VfxShake>();
		shake.Lifetime = lifetime;
		shake.Distance = distance;
	}

	[ClientRpc]
	public void VfxScale(float lifetime, float startScale, float endScale)
	{
		var scale = AddThingComponent<VfxScale>();
		scale.Lifetime = lifetime;
		scale.StartScale = startScale;
		scale.EndScale = endScale;
	}

	[ClientRpc]
	public void VfxSpin(float lifetime, float startAngle, float endAngle)
	{
		var scale = AddThingComponent<VfxSpin>();
		scale.Lifetime = lifetime;
		scale.StartAngle = startAngle;
		scale.EndAngle = endAngle;
	}

    public override int GetHashCode()
    {
        return HashCode.Combine(DisplayIcon, WieldedThing?.DisplayIcon ?? "", PlayerNum + ThingId, RotationDegrees, IconScale, IconDepth, Flags);
        //return HashCode.Combine((DisplayIcon + ThingId.ToString()), PlayerNum, Offset, RotationDegrees, IconScale, IconDepth, Flags);
    }

	public int GetInfoDisplayHash()
    {
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

		if(WieldedThing != null)
		{
			OnNoLongerWieldingThing(thing);
			WieldedThing.OnNoLongerWieldedBy(this);
		}

        WieldedThing = thing;

		if(WieldedThing != null)
		{
            OnWieldThing(thing);
            thing.OnWieldedBy(this);
        }
	}

	public void EquipThing(Thing thing)
	{
		if(EquippedThings == null)
            EquippedThings = new List<Thing>();

		EquippedThings.Add(thing);

        OnEquipThing(thing);
		thing.OnEquippedTo(this);
	}

	public void UnequipThing(Thing thing)
	{
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
        EquippedThings.Clear();
        WieldedThing = null;
		ThingWieldingThis = null;
    }

    public virtual void OnWieldThing(Thing thing) { }
    public virtual void OnNoLongerWieldingThing(Thing thing) { }

    public virtual void OnWieldedBy(Thing thing) 
	{
		ThingWieldingThis = thing;
	}

    public virtual void OnNoLongerWieldedBy(Thing thing) 
	{
		if(thing == ThingWieldingThis)
            ThingWieldingThis = null;
    }

    public virtual void OnEquipThing(Thing thing) {}
    public virtual void OnUnequipThing(Thing thing) {}
    public virtual void OnEquippedTo(Thing thing) {}
    public virtual void OnUnequippedFrom(Thing thing) {}
    public virtual void OnActionRecharged() {}
}
