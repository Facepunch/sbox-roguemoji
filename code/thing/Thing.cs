using Sandbox;
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
	Food = 8,
	Animal = 16,
}

public enum StatType { Strength, Speed, Vitality, Intelligence, Charisma, Sight, Smell, Hearing }

public partial class Stat : BaseNetworkable
{
	[Net] public int CurrentValue { get; set; }
    [Net] public int MinValue { get; set; }
    [Net] public int MaxValue { get; set; }
}

public partial class Thing : Entity
{
	[Net] public IntVector GridPos { get; protected set; }
	[Net] public GridManager ContainingGridManager { get; set; }

    [Net] public string DisplayIcon { get; protected set; }
	[Net] public string DisplayName { get; protected set; }
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
    [Net] public int Hp { get; set; }
    [Net] public int MaxHp { get; set; }

    [Net] public Thing WieldedThing { get; protected set; }

	[Net] public int SightBlockAmount { get; set; }

	[Net] public bool HasStats { get; private set; }
	[Net] public IDictionary<StatType, Stat> Stats { get; private set; }

	[Net] public IList<Thing> EquippedThings { get; private set; }

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
		Assert.True(direction != Direction.None);

		IntVector vec = GridManager.GetIntVectorForDirection(direction);
		IntVector newGridPos = GridPos + vec;

		if ( !ContainingGridManager.IsGridPosInBounds( newGridPos ) )
			return false;

		Thing other = ContainingGridManager.GetThingsAt(newGridPos).WithAll(ThingFlags.Solid).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        if (other != null)
		{
            Interact(other, direction);
            RoguemojiGame.Instance.LogMessage(DisplayIcon + "(" + DisplayName + ") bumped into " + other.DisplayIcon + "(" + other.DisplayName + ")", PlayerNum);

			return false;
		}

		SetGridPos(newGridPos);

		if(shouldAnimate)
			VfxSlide(direction, 0.2f, 40f);

		return true;
	}

    public virtual void PerformedAction()
    {
        
    }

    public virtual void Interact(Thing other, Direction direction)
	{
        VfxNudge(direction, 0.1f, 10f);
        other.VfxShake(0.2f, 4f);

        var explosion = ContainingGridManager.SpawnThing<Explosion>(other.GridPos);
        explosion.VfxShake(0.15f, 6f);
        explosion.VfxScale(0.15f, 0.5f, 1f);

		if (other.MaxHp > 0)
		{
			other.Damage(1, this);
		}
    }

	public virtual void Damage(int amount, Thing source)
	{
		Hp = Math.Clamp(Hp - amount, 0, MaxHp);

		if(Hp <= 0)
		{
			Destroy();
		}
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
		if (Host.IsServer)
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
		return HashCode.Combine(NetworkIdent, DisplayIcon, WieldedThing?.DisplayIcon ?? "", Hp, MaxHp, Flags);
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

		Log.Info("UnequipThing: " + EquippedThings.Count());

        OnUnequipThing(thing);
		thing.OnUnequippedFrom(this);
    }
	
	public virtual void InitStat(StatType statType, int current, int min, int max)
	{
		if (!HasStats)
		{
			Stats = new Dictionary<StatType, Stat>();
			HasStats = true;
		}

        Stats[statType] = new Stat()
		{
			CurrentValue = current,
			MinValue = min,
			MaxValue = max
		};
    }

    public void AdjustStat(StatType statType, int amount)
	{
		if (HasStats && Stats.ContainsKey(statType))
		{
            Stats[statType].CurrentValue += amount;
            ChangedStat(statType);
        }
    }

    public void AdjustStatMin(StatType statType, int amount)
    {
        if (HasStats && Stats.ContainsKey(statType))
		{
            Stats[statType].MinValue += amount;
            ChangedStat(statType);
        }
    }

    public void AdjustStatMax(StatType statType, int amount)
    {
        if (HasStats && Stats.ContainsKey(statType))
		{
            Stats[statType].MaxValue += amount;
			ChangedStat(statType);
        }
    }

	public virtual void ChangedStat(StatType statType) { }

    public int GetStat(StatType statType)
	{
		if (HasStats && Stats.ContainsKey(statType))
		{
			var stat = Stats[statType];
			return Math.Clamp(stat.CurrentValue, stat.MinValue, stat.MaxValue);
        }

		return 0;
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
    }

    public virtual void OnWieldThing(Thing thing) { }
    public virtual void OnNoLongerWieldingThing(Thing thing) { }
    public virtual void OnWieldedBy(Thing thing) { }
    public virtual void OnNoLongerWieldedBy(Thing thing) { }
    public virtual void OnEquipThing(Thing thing) {}
    public virtual void OnUnequipThing(Thing thing) {}
    public virtual void OnEquippedTo(Thing thing) {}
    public virtual void OnUnequippedFrom(Thing thing) {}
}
