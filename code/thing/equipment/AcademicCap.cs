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
        IconDepth = 0;
		Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Equipment;

        if (Game.IsServer)
        {
            IntelligenceAmount = 2;
            InitStat(StatType.Intelligence, IntelligenceAmount, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        thing.AdjustStat(StatType.Intelligence, IntelligenceAmount);

        if (thing is RoguemojiPlayer && thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("🤓", (int)PlayerIconPriority.AcademicCapNerd, 1.0f);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        thing.AdjustStat(StatType.Intelligence, -IntelligenceAmount);
    }
}
