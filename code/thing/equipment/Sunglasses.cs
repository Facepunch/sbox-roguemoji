using Sandbox;
using System;

namespace Roguemoji;
public partial class Sunglasses : Thing
{
	public Sunglasses()
	{
		DisplayIcon = "🕶️";
        DisplayName = "Sunglasses";
        Description = "Looks cool but obscures your vision.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "Dark sunglasses.";
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;

        if (Game.IsServer)
        {
            InitStat(StatType.Charisma, 3);
            InitStat(StatType.Sight, -3);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        foreach(var pair in Stats)
            thing.AdjustStat(pair.Key, pair.Value.CurrentValue);

        thing.SetIcon("😎");
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        foreach (var pair in Stats)
            thing.AdjustStat(pair.Key, -pair.Value.CurrentValue);

        if (!thing.HasEquipmentType(TypeLibrary.GetType(typeof(Sunglasses))))
            thing.SetIcon("😀");
    }
}
