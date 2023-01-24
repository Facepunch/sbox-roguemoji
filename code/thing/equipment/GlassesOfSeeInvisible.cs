using Sandbox;
using System;

namespace Roguemoji;
public partial class GlassesOfSeeInvisible : Thing
{
    public int IconId { get; set; }

    public GlassesOfSeeInvisible()
	{
		DisplayIcon = "👓️";
        DisplayName = "Glasses of See Invisible";
        Description = "Allows you to see invisible things";
        Tooltip = "Glasses of See Invisible";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Equipment;

        if (Game.IsServer)
        {
            AddTrait("", Globals.Icon(IconType.Invisible), $"Reveals invisible things", offset: new Vector2(0f, -1f), tattooIcon: "👁️‍🗨️", tattooOffset: new Vector2(8f, 8f), tattooScale: 0.7f);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        if (!thing.HasStat(StatType.SeeInvisible))
            thing.InitStat(StatType.SeeInvisible, 1);
        else
            thing.AdjustStat(StatType.SeeInvisible, 1);

        if(thing is RoguemojiPlayer player)
        {
            if (thing.GetComponent<CIconPriority>(out var component))
            {
                var iconPriority = (CIconPriority)component;
                IconId = iconPriority.AddIconPriority("🤓", (int)PlayerIconPriority.GlassesOfSeeInvisible);
            }
        }
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        thing.AdjustStat(StatType.SeeInvisible, -1);

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
