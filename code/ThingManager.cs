using System;
using System.Collections.Generic;
using Sandbox;

namespace Interfacer;

public class ThingManager
{
	public static ThingManager Instance { get; private set; }

	public List<Thing> Things = new List<Thing>();

	public ThingManager()
	{
		Instance = this;
	}

	public void Update(float dt)
	{
		for(int i = Things.Count - 1; i >= 0; i--)
		{
			var thing = Things[i];

			if ( !thing.DoneFirstUpdate )
				thing.FirstUpdate();

			if ( thing.ShouldUpdate || thing.Statuses.Count > 0 )
				thing.Update( dt );
		}
	}

	public void AddThing(Thing thing)
	{
		Things.Add(thing);
	}

	public void RemoveThing(Thing thing)
	{
		Things.Remove(thing);
	}
}
