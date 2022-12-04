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
}
public partial class Thing : Entity
{
	[Net] public IntVector GridPos { get; protected set; }
	[Net] public GridManager ContainingGridManager { get; set; }

	//[Net] public string DisplayImagePath { get; protected set; }
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

    [Net] public RoguemojiPlayer InventoryPlayer { get; set; }

	[Net] public string DebugText { get; set; }

    [Net] public int ThingId { get; private set; }

    public Dictionary<TypeDescription, ThingStatus> Statuses = new Dictionary<TypeDescription, ThingStatus>();

	[Net] public ThingFlags Flags { get; set; }
    [Net] public int Hp { get; set; }
    [Net] public int MaxHp { get; set; }

    [Net] public Thing EquippedThing { get; protected set; }

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
		//DisplayImagePath = "textures/emoji/hole.png";
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

		foreach (KeyValuePair<TypeDescription, ThingStatus> pair in Statuses)
		{
			var status = pair.Value;
			if (status.ShouldUpdate)
				status.Update(dt);
		}

		if(!string.IsNullOrEmpty(DebugText))
			DrawDebugText(DebugText);

        Offset = Utils.DynamicEaseTo(Offset, TargetOffset, 0.6f, dt);

        //DrawDebugText(ContainingGridManager?.Name.ToString() ?? "null");
        //DrawDebugText(Flags.ToString());
    }

	public virtual void Update(float dt)
	{
		//DebugText = "Server Statuses (" + Statuses.Count + "):\n";
		foreach (KeyValuePair<TypeDescription, ThingStatus> pair in Statuses)
        {
			var status = pair.Value;
			if (status.ShouldUpdate)
				status.Update(dt);

			//DebugText += status.GetType().Name + "\n";
		}

		//DrawDebugText(Flags.ToString());
	}

	public virtual void FirstUpdate()
	{
		SetGridPos(GridPos, forceRefresh: true);
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

	public virtual void SetGridPos( IntVector gridPos, bool forceRefresh = false )
	{
		if ( GridPos.Equals( gridPos ) && !forceRefresh )
			return;

		ContainingGridManager.SetGridPos( this, gridPos );
		GridPos = gridPos;

		if(ContainingGridManager.GridType == GridType.Inventory || ContainingGridManager.GridType == GridType.Equipment)
            RefreshGridPanelClient(To.Single(InventoryPlayer));
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

	public ThingStatus AddStatus(TypeDescription type)
	{
		if(type == null)
		{
			Log.Info("type is null!");
			return null;
		}

		if(Statuses.ContainsKey(type))
        {
			var status = Statuses[type];
			status.ReInitialize();
			return status;
        }
		else
        {
			var status = type.Create<ThingStatus>();
			status.Init(this);
			Statuses.Add(type, status);
			return status;
		}
	}

	public T AddStatus<T>() where T : ThingStatus
	{
		return AddStatus(TypeLibrary.GetDescription(typeof(T))) as T;
	}

	public void RemoveStatus(TypeDescription type)
    {
		if(Statuses.ContainsKey(type))
        {
			var status = Statuses[type];
			status.OnRemove();
			Statuses.Remove(type);
        }
    }

	public bool GetStatus(TypeDescription type, out ThingStatus status)
	{
		if (Statuses.ContainsKey(type))
		{
			status = Statuses[type];
			return true;
		}

		status = null;
		return false;
	}

	public void ForEachStatus(Action<ThingStatus> action)
	{
		foreach (var (_, status) in Statuses)
		{
			action(status);
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
		var nudge = AddStatus<VfxNudgeStatus>();
        nudge.Direction = direction;
        nudge.Lifetime = lifetime;
        nudge.Distance = distance;
    }

	[ClientRpc]
	public void VfxSlide(Direction direction, float lifetime, float distance)
    {
		var slide = AddStatus<VfxSlideStatus>();
		slide.Direction = direction;
		slide.Lifetime = lifetime;
		slide.Distance = distance;
	}

	[ClientRpc]
	public void VfxShake(float lifetime, float distance)
	{
		var shake = AddStatus<VfxShakeStatus>();
		shake.Lifetime = lifetime;
		shake.Distance = distance;
	}

	[ClientRpc]
	public void VfxScale(float lifetime, float startScale, float endScale)
	{
		var scale = AddStatus<VfxScaleStatus>();
		scale.Lifetime = lifetime;
		scale.StartScale = startScale;
		scale.EndScale = endScale;
	}

	[ClientRpc]
	public void VfxSpin(float lifetime, float startAngle, float endAngle)
	{
		var scale = AddStatus<VfxSpinStatus>();
		scale.Lifetime = lifetime;
		scale.StartAngle = startAngle;
		scale.EndAngle = endAngle;
	}

    public override int GetHashCode()
    {
        return HashCode.Combine(DisplayIcon, EquippedThing?.DisplayIcon ?? "", PlayerNum + ThingId, RotationDegrees, IconScale, IconDepth, Flags);
        //return HashCode.Combine((DisplayIcon + ThingId.ToString()), PlayerNum, Offset, RotationDegrees, IconScale, IconDepth, Flags);
    }

	public int GetInfoDisplayHash()
    {
		return HashCode.Combine(NetworkIdent, DisplayIcon, EquippedThing?.DisplayIcon ?? "", Hp, MaxHp, Flags);
    }

    public int GetNearbyCellHash()
    {
        return HashCode.Combine(DisplayIcon, PlayerNum, IconDepth, Flags, NetworkIdent, ThingId);
    }

	public int GetZPos()
	{
		return (IconDepth * 100) + StackNum;
    }

	public virtual void EquipThing(Thing thing)
	{
		EquippedThing = thing;
	}
}
