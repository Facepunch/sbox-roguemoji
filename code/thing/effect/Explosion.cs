using Sandbox;
using System;

namespace Roguemoji;
public partial class Explosion : Thing
{
	private TimeSince _spawnTime;
	private const float LIFE_TIME = 0.15f;

	public Explosion()
	{
        DisplayIcon = "💥";
		IconDepth = 9;
		_spawnTime = 0f;
		ShouldUpdate = true;
	}

	public override void Update(float dt)
	{
		base.Update( dt );

		if( _spawnTime > LIFE_TIME )
			Destroy();
	}
}
