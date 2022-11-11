using Sandbox;
using System;

namespace Interfacer;
public partial class InterfacerPlayer : Thing
{
	private TimeSince _inputRepeatTime;
	private const float REPEAT_DELAY = 0.125f;

	public override string DisplayName => Client?.Name ?? "Player";

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

	}

	public override void Simulate( Client cl )
	{
		if(Host.IsServer)
		{
			if ( Input.Pressed( InputButton.Left ) || (_inputRepeatTime > REPEAT_DELAY && Input.Down( InputButton.Left )) )
				TryMove(Direction.Left);
			else if ( Input.Pressed( InputButton.Right ) || (_inputRepeatTime > REPEAT_DELAY && Input.Down( InputButton.Right )) )
				TryMove( Direction.Right );
			else if ( Input.Pressed( InputButton.Back ) || (_inputRepeatTime > REPEAT_DELAY && Input.Down( InputButton.Back )) )
				TryMove( Direction.Down);
			else if ( Input.Pressed( InputButton.Forward ) || (_inputRepeatTime > REPEAT_DELAY && Input.Down( InputButton.Forward )) )
				TryMove( Direction.Up);
		}
	}

	public override bool TryMove( Direction direction )
	{
		var success = base.TryMove( direction );
		if ( success )
			_inputRepeatTime = 0f;

		return success;
	}

	string GetPlayerIcon(int playerNum)
	{
		switch (playerNum) {
			case 0:
			default:
				return "."; 
			case 1:
				return "🐻";
			case 2:
				return "🐭";
			case 3:
				return "🐵";
			case 4:
				return "🐷";
			case 5:
				return "🐮";
			case 6:
				return "🐱";
			case 7:
				return "🐶";
			case 8:
				return "🐹";
		}
	}
}
