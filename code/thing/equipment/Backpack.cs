using Sandbox;
using System;

namespace Roguemoji;
public partial class Backpack : Thing
{
    public Backpack()
	{
		DisplayIcon = "🎒";
        DisplayName = "Backpack";
        Description = "Increases your inventory size";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A backpack";
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;
    }

    public override void OnEquippedTo(Thing thing)
    {
        if(thing is RoguemojiPlayer player)
            player.InventoryGridManager.SetWidth(player.InventoryGridManager.GridWidth + 1);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        if (thing is RoguemojiPlayer player)
            player.InventoryGridManager.SetWidth(player.InventoryGridManager.GridWidth - 1);
    }
}
