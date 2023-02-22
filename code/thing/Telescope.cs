using Sandbox;
using System;

namespace Roguemoji;
public partial class Telescope : Thing
{
    public int SightAmount { get; private set; }
    public int SpeedAmount { get; private set; }
    public float CooldownTime { get; private set; }

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

            CooldownTime = 20f;

            AddTrait("", "⏳", $"Cooldown time: {CooldownTime}s", offset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        base.OnEquippedTo(thing);

        thing.AdjustStat(StatType.SightDistance, SightAmount);
        thing.AdjustStat(StatType.Speed, SpeedAmount);

        StartCooldown(CooldownTime);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        base.OnUnequippedFrom(thing);

        thing.AdjustStat(StatType.SightDistance, -SightAmount);
        thing.AdjustStat(StatType.Speed, -SpeedAmount);

        StartCooldown(CooldownTime);
    }
}
