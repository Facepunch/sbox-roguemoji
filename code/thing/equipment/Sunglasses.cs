using Sandbox;
using System;

namespace Roguemoji;
public partial class Sunglasses : Thing
{
    public int CharismaAmount { get; private set; }
    public int SightAmount { get; private set; }
    public int IconId { get; set; }

    public Sunglasses()
	{
		DisplayIcon = "🕶️";
        DisplayName = "Sunglasses";
        Description = "Obscures your vision but looks cool";
        Tooltip = "A pair of sunglasses";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Equipment;
        Flammability = 13;

        if (Game.IsServer)
        {
            CharismaAmount = 3;
            SightAmount = -2;

            InitStat(StatType.Charisma, CharismaAmount, isModifier: true);
            InitStat(StatType.SightDistance, SightAmount, min: -999, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        base.OnEquippedTo(thing);

        foreach (var pair in Stats)
            thing.AdjustStat(pair.Key, pair.Value.CurrentValue);

        if(thing is Smiley smiley)
        {
            if (thing.GetComponent<CIconPriority>(out var component))
            {
                var iconPriority = (CIconPriority)component;
                IconId = iconPriority.AddIconPriority("😎", (int)PlayerIconPriority.Sunglasses);
            }
        }
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        base.OnUnequippedFrom(thing);

        foreach (var pair in Stats)
            thing.AdjustStat(pair.Key, -pair.Value.CurrentValue);

        if (thing is Smiley smiley)
        {
            if (thing.GetComponent<CIconPriority>(out var component))
            {
                var iconPriority = (CIconPriority)component;
                iconPriority.RemoveIconPriority(IconId);
            }
        }
    }
}
