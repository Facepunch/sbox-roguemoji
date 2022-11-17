using Sandbox;
using System;
using System.Collections.Generic;

namespace Interfacer;
public partial class Thing : Entity
{
	[Net] public IntVector GridPos { get; set; }
	[Net] public GridPanelType GridPanelType { get; set; }
	public GridManager GridManager => InterfacerGame.Instance.GetGridManager(GridPanelType);

	[Net] public string DisplayIcon { get; protected set; }
	[Net] public string DisplayName { get; protected set; }
	[Net] public string Tooltip { get; protected set; }

	public bool ShouldUpdate { get; set; }
	public bool DoneFirstUpdate { get; private set; }

	[Net] public int PlayerNum { get; set; }

	public float IconPriority { get; set; }
	public bool IsVisualEffect { get; set; }
	public bool ShouldLogBehaviour { get; set; }

	public Vector2 Offset { get; set; }
	public float RotationDegrees { get; set; }
	public float IconScale { get; set; }

	public Dictionary<TypeDescription, ThingStatus> Statuses = new Dictionary<TypeDescription, ThingStatus>();

	public Thing()
	{
		ShouldUpdate = false;
		DisplayIcon = ".";
		DisplayName = Name;
		Tooltip = "";
		IconPriority = 0f;
		ShouldLogBehaviour = false;
		IconScale = 1f;
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	[Event.Tick.Client]
	public void ClientTick()
    {
		//Log.Info(Name + " ClientTick: " + Time.Delta + " DisplayIcon: " + DisplayIcon + " GridPos: " + GridPos + " PlayerNum: " + PlayerNum + " DisplayName: " + DisplayName);
    }

    public virtual void Update(float dt)
	{
		var statusString = "Statuses (" + Statuses.Count + "):\n";
		foreach (KeyValuePair<TypeDescription, ThingStatus> pair in Statuses)
        {
			var status = pair.Value;
			if (status.ShouldUpdate)
				status.Update(dt);

			statusString += status.GetType().Name + "\n";
		}

		//DrawDebugText(statusString);
    }

	public virtual void FirstUpdate()
	{
		SetGridPos(GridPos, forceRefresh: true);
		DoneFirstUpdate = true;
	}

	public virtual bool TryMove(Direction direction)
	{
		IntVector vec = GridManager.GetIntVectorForDirection(direction);
		IntVector newGridPos = GridPos + vec;

		if ( !GridManager.IsGridPosInBounds( newGridPos ) )
			return false;

		if (GridManager.DoesThingExistAt( newGridPos ) )
		{
			var otherThing = GridManager.GetThingAt( newGridPos );
			if(!otherThing.IsVisualEffect)
			{
				var pushSuccess = otherThing.TryMove( direction );
				if ( !pushSuccess )
                {
                    otherThing.VfxShake(0.2f, 4f);
					return false;
				}

				if ( ShouldLogBehaviour )
					InterfacerGame.Instance.LogMessage( DisplayIcon + "(" + DisplayName + ") pushed " + otherThing.DisplayIcon + " " + GridManager.GetDirectionText(direction) + "!", PlayerNum );

				if (!GridManager.DoesThingExistAt(newGridPos))
                {
					var explosion = new Explosion() 
					{ 
						GridPos = newGridPos,
						GridPanelType = GridPanelType,
					};
                    explosion.VfxShake(0.15f, 5f);
                    explosion.VfxScale(2.15f, 1f, 2.8f);
					ThingManager.Instance.AddThing(explosion);
				}

				return false;
			}
		}

		SetGridPos(newGridPos);
		VfxSlide(direction, 0.2f, 40f);

		return true;
	}

	public void SetGridPos( IntVector gridPos, bool forceRefresh = false )
	{
		if ( GridPos.Equals( gridPos ) && !forceRefresh )
			return;

		if (GridPanelType == GridPanelType.None)
			Log.Error(DisplayName + " has no GridPanelType!");

		GridManager.SetGridPos( this, gridPos );
		GridPos = gridPos;

		GridPosChangedClient();

		if (ShouldLogBehaviour)
			InterfacerGame.Instance.LogMessage( DisplayIcon + "(" + DisplayName + ") moved to (" + gridPos.x + ", " + gridPos.y + ").", PlayerNum);
	}

	[ClientRpc]
	public void GridPosChangedClient()
    {
		if (Hud.Instance == null)
			return;

		var panel = Hud.Instance.GetGridPanel(GridPanelType);
		if (panel == null)
			return;

		panel.Refresh();
    }

	public void Remove()
	{
		if ( ShouldLogBehaviour )
			InterfacerGame.Instance.LogMessage( DisplayIcon + "(" + DisplayName + ") removed.", PlayerNum );

		GridManager.DeregisterGridPos( this, GridPos );
		ThingManager.Instance.RemoveThing( this );
		Delete();
	}

	public void SetOffset(Vector2 offset)
    {
		Offset = offset;
		GridManager.RefreshGridPos(GridPos);
    }

	public void SetRotation(float rotationDegrees)
	{
		RotationDegrees = rotationDegrees;
		GridManager.RefreshGridPos(GridPos);
	}

	public void SetScale(float scale)
	{
		IconScale = scale;
		GridManager.RefreshGridPos(GridPos);
	}

	public ThingStatus AddStatus(TypeDescription type)
	{
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

	public void RemoveStatus(TypeDescription type)
    {
		if(Statuses.ContainsKey(type))
        {
			var status = Statuses[type];
			status.OnRemove();
			Statuses.Remove(type);
        }
    }

	public void ForEachStatus(Action<ThingStatus> action)
	{
		foreach (var (_, status) in Statuses)
		{
			action(status);
		}
	}

	public void DrawDebugText(string text, Color color, float time = 0f)
    {
		DebugOverlay.ScreenText(text, GridManager.GetScreenPos(GridPos), 0, color, time);
	}

	public void DrawDebugText(string text)
	{
		DrawDebugText(text, new Color(1f, 1f, 1f, 0.5f));
	}

	public void SetIcon(string icon)
	{
		DisplayIcon = icon;
		GridManager.RefreshGridPos(GridPos);
	}

	public void VfxNudge(Direction direction, float lifetime, float distance)
	{
		//InterfacerGame.Instance.VfxNudgeClient(GridPanelType, GridPos.x, GridPos.y, direction, lifetime, distance);
	}

	public void VfxSlide(Direction direction, float lifetime, float distance)
    {
		//InterfacerGame.Instance.VfxSlideClient(GridPanelType, GridPos.x, GridPos.y, direction, lifetime, distance);
	}

	public void VfxShake(float lifetime, float distance)
	{
		//InterfacerGame.Instance.VfxShakeClient(GridPanelType, GridPos.x, GridPos.y, lifetime, distance);
	}

	public void VfxScale(float lifetime, float startScale, float endScale)
	{
		//InterfacerGame.Instance.VfxScaleClient(GridPanelType, GridPos.x, GridPos.y, lifetime, startScale, endScale);
	}
}
