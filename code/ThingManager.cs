using System;
using System.Collections.Generic;
using Sandbox;

namespace Interfacer;

public partial class ThingManager : BaseNetworkable
{
	public static ThingManager Instance { get; private set; }

	[Net] public IList<Thing> ThingsArena { get; private set; }
	[Net] public IList<Thing> ThingsInventory { get; private set; }

	[Net] public Thing SelectedThing { get; private set; }

	public ThingManager()
	{
		Instance = this;
		ThingsArena = new List<Thing>();
		ThingsArena = new List<Thing>();
	}

	public void Update(float dt)
	{
		UpdateThings(ThingsArena, dt);
		UpdateThings(ThingsInventory, dt);
	}

	void UpdateThings(IList<Thing> things, float dt)
    {
		for (int i = things.Count - 1; i >= 0; i--)
		{
			var thing = things[i];

			if (!thing.DoneFirstUpdate)
				thing.FirstUpdate();

			if (thing.ShouldUpdate || thing.Statuses.Count > 0)
				thing.Update(dt);
		}
	}

	public void AddThing(Thing thing)
	{
		if (thing.GridPanelType == GridPanelType.Arena)
			ThingsArena.Add(thing);
		else if (thing.GridPanelType == GridPanelType.Inventory)
			ThingsInventory.Add(thing);
		else
			Log.Error("ThingManager - AddThing: " + thing.DisplayName + " has GridPanelType: " + thing.GridPanelType + "!");
	}

	public void RemoveThing(Thing thing)
	{
		if (thing.GridPanelType == GridPanelType.Arena)
			ThingsArena.Remove(thing);
		else if (thing.GridPanelType == GridPanelType.Inventory)
			ThingsInventory.Remove(thing);
		else
			Log.Error("ThingManager - RemoveThing: " + thing.DisplayName + " has GridPanelType: " + thing.GridPanelType + "!");
	}

	public IList<Thing> GetThings(GridPanelType gridPanelType)
    {
		if (gridPanelType == GridPanelType.Arena)
			return ThingsArena;
		else if (gridPanelType == GridPanelType.Inventory)
			return ThingsInventory;

		Log.Error("ThingManager - GetThings: GridPanelType." + gridPanelType + "!");
		return null;
	}

	public void SelectThing(Thing thing)
    {
		SelectedThing = thing;
    }
}
