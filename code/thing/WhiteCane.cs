using Sandbox;
using System;

namespace Roguemoji;
public partial class WhiteCane : Thing
{
    public Trait Trait { get; private set; }

	public WhiteCane()
	{
		DisplayIcon = "🦯";
        DisplayName = "White Cane";
        Description = "Useful when you can't see anything.";
        Tooltip = "A white cane for the vision impaired.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
            AddTrait("", "👁️", "Prevents your sight from reaching zero.");
        }
    }

    public override void OnWieldedBy(Thing thing)
    {
        thing.AdjustStatMin(StatType.Sight, 3);
        thing.AdjustStat(StatType.Attack, GetStatClamped(StatType.Attack));
        Trait = thing.AddTrait("", "👁️", "Your sight can't go down to zero.", DisplayName);
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        thing.AdjustStatMin(StatType.Sight, -3);
        thing.AdjustStat(StatType.Attack, -GetStatClamped(StatType.Attack));
        thing.RemoveTrait(Trait);
    }
}
