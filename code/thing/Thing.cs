using Sandbox;
using System;
using System.Collections.Generic;

namespace Interfacer;
public partial class Thing : Entity
{
	public IntVector GridPos { get; set; }

	public virtual string DisplayIcon { get; protected set; }
	public virtual string DisplayName => Name;
	public virtual string Tooltip => "";

	public bool ShouldUpdate { get; set; }
	public bool DoneFirstUpdate { get; private set; }

	public int PlayerNum { get; set; }

	public float IconPriority { get; set; }
	public bool IsVisualEffect { get; set; }
	public bool ShouldLogBehaviour { get; set; }

	public Vector2 Offset { get; set; }
	public float RotationDegrees { get; set; }

	public Dictionary<TypeDescription, ThingStatus> Statuses = new Dictionary<TypeDescription, ThingStatus>();

	public Thing()
	{
		ShouldUpdate = false;
		DisplayIcon = ".";
		IconPriority = 0f;
		ShouldLogBehaviour = false;
	}

	public virtual void Update(float dt)
	{
		var statusString = "";
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

		if ( !GridManager.Instance.IsGridPosInBounds( newGridPos ) )
			return false;

		if ( GridManager.Instance.DoesThingExistAt( newGridPos ) )
		{
			var otherThing = GridManager.Instance.GetThingAt( newGridPos );
			if(!otherThing.IsVisualEffect)
			{
				var pushSuccess = otherThing.TryMove( direction );
				if ( !pushSuccess )
					return false;

				//otherThing.VfxSlide(direction, 0.2f, 20f);
				otherThing.VfxShake(0.3f, 5f);

				if ( ShouldLogBehaviour )
					InterfacerGame.Instance.LogMessage( DisplayIcon + "(" + DisplayName + ") pushed " + otherThing.DisplayIcon + " " + GridManager.GetDirectionText(direction) + "!", PlayerNum );

				if (!GridManager.Instance.DoesThingExistAt(newGridPos))
                {
					var explosion = new Explosion() { GridPos = newGridPos };
					ThingManager.Instance.AddThing(explosion);
				}

				return false;
			}
		}

		SetGridPos( newGridPos );
		return true;
	}

	public void SetGridPos( IntVector gridPos, bool forceRefresh = false )
	{
		if ( GridPos.Equals( gridPos ) && !forceRefresh )
			return;

		GridManager.Instance.SetGridPos( this, gridPos );
		GridPos = gridPos;

		if(ShouldLogBehaviour)
			InterfacerGame.Instance.LogMessage( DisplayIcon + "(" + DisplayName + ") moved to (" + gridPos.x + ", " + gridPos.y + ").", PlayerNum);
	}

	public void Remove()
	{
		if ( ShouldLogBehaviour )
			InterfacerGame.Instance.LogMessage( DisplayIcon + "(" + DisplayName + ") removed.", PlayerNum );

		GridManager.Instance.DeregisterGridPos( this, GridPos );
		ThingManager.Instance.RemoveThing( this );
		Delete();
	}

	public void SetOffset(Vector2 offset)
    {
		Offset = offset;
		GridManager.Instance.RefreshGridPos(GridPos);
    }

	public void SetRotation(float rotationDegrees)
	{
		RotationDegrees = rotationDegrees;
		GridManager.Instance.RefreshGridPos(GridPos);
	}

	public ThingStatus AddStatus(TypeDescription type)
	{
		Log.Info(DisplayName + " AddStatus: " + type.Name);

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
			Log.Info(DisplayName + " RemoveStatus: " + type.Name);

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
		DebugOverlay.ScreenText(text, GridManager.Instance.GetScreenPos(GridPos), 0, color, time);
	}

	public void DrawDebugText(string text)
	{
		DrawDebugText(text, Color.White);
	}

	public void VfxNudge(Direction direction, float lifetime, float distance)
	{
		var nudge = AddStatus(TypeLibrary.GetDescription(typeof(VfxNudgeStatus))) as VfxNudgeStatus;
		nudge.Direction = direction;
		nudge.Lifetime = lifetime;
		nudge.Distance = distance;
	}

	public void VfxSlide(Direction direction, float lifetime, float distance)
    {
		var slide = AddStatus(TypeLibrary.GetDescription(typeof(VfxSlideStatus))) as VfxSlideStatus;
		slide.Direction = direction;
		slide.Lifetime = lifetime;
		slide.Distance = distance;
	}

	public void VfxShake(float lifetime, float distance)
	{
		var shake = AddStatus(TypeLibrary.GetDescription(typeof(VfxShakeStatus))) as VfxShakeStatus;
		shake.Lifetime = lifetime;
		shake.Distance = distance;
	}
}
