using Sandbox;
using System;

namespace Roguemoji;
public partial class Telescope : Thing
{
    public int SightAmount { get; private set; }
    public int SpeedAmount { get; private set; }

    // todo: see farther, but dont see through more things

    public Telescope()
	{
		DisplayIcon = "🔭";
        DisplayName = "Telescope";
        Description = "See farther but move slower";
        Tooltip = "A telescope";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Equipment;
        Flammability = 9;

        if (Game.IsServer)
        {
            SightAmount = 4;
            SpeedAmount = -3;
            InitStat(StatType.SightDistance, SightAmount, min: -999, isModifier: true);
            InitStat(StatType.Speed, SpeedAmount, min: -999, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        base.OnEquippedTo(thing);

        thing.AdjustStat(StatType.SightDistance, SightAmount);
        thing.AdjustStat(StatType.Speed, SpeedAmount);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        base.OnUnequippedFrom(thing);

        thing.AdjustStat(StatType.SightDistance, -SightAmount);
        thing.AdjustStat(StatType.Speed, -SpeedAmount);
    }
}
