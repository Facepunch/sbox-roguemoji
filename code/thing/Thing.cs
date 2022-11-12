using Sandbox;
using System;

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

	public Thing()
	{
		ShouldUpdate = false;
		DisplayIcon = ".";
		IconPriority = 0f;
		ShouldLogBehaviour = false;
	}

	public virtual void Update(float dt)
	{
		
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

				if ( ShouldLogBehaviour )
					InterfacerGame.Instance.LogMessage( DisplayIcon + "(" + DisplayName + ") pushed " + otherThing.DisplayIcon + " " + GridManager.GetDirectionText(direction) + "!", PlayerNum );

				var explosion = new Explosion()
				{
					GridPos = newGridPos,
				};
				ThingManager.Instance.AddThing( explosion );

				return true;
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
}
