using Sandbox;
using System;

namespace Interfacer;
public partial class InterfacerPlayer : Thing
{
	private TimeSince _inputRepeatTime;
	private const float MOVE_DELAY = 0.3f;

	public override string DisplayName => Client?.Name ?? "Player";
	public override string Tooltip => Client?.Name ?? "Player";

	public InterfacerPlayer()
	{
		ShouldUpdate = true;
		PlayerNum = ++InterfacerGame.PlayerNum;
		DisplayIcon = "🙂";
		IconPriority = 2f;
		ShouldLogBehaviour = true;
	}

	public override void Spawn()
	{
		base.Spawn();

	}

	public override void Update( float dt )
	{
		base.Update( dt );

		//var sine = Utils.Map(MathF.Sin(Time.Now * 4f), -1f, 1f, -1f, 1f, EasingType.ExpoInOut);
		//SetOffset(new Vector2(sine * 3f, 0f));
		//SetRotation(sine * 25f);
	}

	public override void Simulate( Client cl )
	{
		if(Host.IsServer)
		{
			if (_inputRepeatTime > MOVE_DELAY)
            {
				if (Input.Down(InputButton.Left))
					TryMove(Direction.Left);
				else if (Input.Down(InputButton.Right))
					TryMove(Direction.Right);
				else if (Input.Down(InputButton.Back))
					TryMove(Direction.Down);
				else if (Input.Down(InputButton.Forward))
					TryMove(Direction.Up);
			}
		}
	}

	public override bool TryMove( Direction direction )
	{
		var success = base.TryMove( direction );
		if (success)
		{
			SetIcon("😀");
		}
		else 
		{
			SetIcon("🤨");
            VfxNudge(direction, 0.1f, 10f);
		}
			
		_inputRepeatTime = 0f;

		return success;
	}
}
