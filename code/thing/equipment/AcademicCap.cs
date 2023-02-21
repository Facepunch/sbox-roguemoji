using Sandbox;
using System;

namespace Roguemoji;
public partial class AcademicCap : Thing
{
    public int IntelligenceAmount { get; private set; }

    public AcademicCap()
	{
		DisplayIcon = "🎓";
        DisplayName = "Academic Cap";
        Description = "Makes you feel smarter";
        Tooltip = "An academic cap";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Equipment;
        Flammability = 24;

        if (Game.IsServer)
        {
            IntelligenceAmount = 2;
            InitStat(StatType.Intelligence, IntelligenceAmount, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        base.OnEquippedTo(thing);

        thing.AdjustStat(StatType.Intelligence, IntelligenceAmount);

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("🤓", (int)PlayerIconPriority.AcademicCapNerd, 1.0f);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        base.OnUnequippedFrom(thing);

        thing.AdjustStat(StatType.Intelligence, -IntelligenceAmount);
    }
}
