using Sandbox;
using System;

namespace Roguemoji;
public partial class Sunglasses : Thing
{
    public int CharismaAmount { get; private set; }
    public int SightAmount { get; private set; }

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
            CharismaAmount = 3;
            SightAmount = -2;

            InitStat(StatType.Charisma, CharismaAmount, isModifier: true);
            InitStat(StatType.Sight, SightAmount, int.MinValue, isModifier: true);
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
