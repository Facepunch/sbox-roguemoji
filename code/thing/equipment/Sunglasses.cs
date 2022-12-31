using Sandbox;
using System;

namespace Roguemoji;
public partial class Sunglasses : Thing
{
	public Sunglasses()
	{
		DisplayIcon = "🕶️";
        DisplayName = "Sunglasses";
        Description = "Obscures your vision but looks cool.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "Dark sunglasses.";
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;

        if (Game.IsServer)
        {
            InitStat(StatType.Charisma, 3, isModifier: true);
            InitStat(StatType.Sight, -3, int.MinValue, isModifier: true);
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
