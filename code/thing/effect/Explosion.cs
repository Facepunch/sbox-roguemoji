using Sandbox;
using System;

namespace Interfacer;
public partial class Explosion : Thing
{
	private TimeSince _spawnTime;
	private const float LIFE_TIME = 0.15f;

	public Explosion()
	{
		DisplayIcon = "💥";
		IconPriority = 99f;
		ShouldUpdate = true;
		_spawnTime = 0f;
		IsVisualEffect = true;
	}

	public override void Update( float dt )
	{
		base.Update( dt );

		if( _spawnTime > LIFE_TIME )
			Remove();
	}
}
