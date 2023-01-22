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
        IconDepth = 0;
		Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Equipment;

        if (Game.IsServer)
        {
            CharismaAmount = 3;
            SightAmount = -2;

            InitStat(StatType.Charisma, CharismaAmount, isModifier: true);
            InitStat(StatType.Sight, SightAmount, min: -999, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        foreach(var pair in Stats)
            thing.AdjustStat(pair.Key, pair.Value.CurrentValue);

        if(thing is RoguemojiPlayer player)
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
        foreach (var pair in Stats)
            thing.AdjustStat(pair.Key, -pair.Value.CurrentValue);

        if (thing is RoguemojiPlayer player)
        {
            if (thing.GetComponent<CIconPriority>(out var component))
            {
                var iconPriority = (CIconPriority)component;
                iconPriority.RemoveIconPriority(IconId);
            }
        }
    }
}
