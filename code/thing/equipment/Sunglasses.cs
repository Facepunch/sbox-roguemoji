using Sandbox;
using System;

namespace Roguemoji;
public partial class Sunglasses : Thing
{
	public Sunglasses()
	{
		DisplayIcon = "🕶️";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "Dark sunglasses.";
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;
    }

    public override void OnEquippedTo(Thing thing)
    {
        thing.AdjustStat(StatType.Sight, -3);
        thing.SetIcon("😎");
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        thing.AdjustStat(StatType.Sight, 3);

        if (!thing.HasEquipmentType(TypeLibrary.GetType(typeof(Sunglasses))))
            thing.SetIcon("😀");
    }
}
