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
        Tooltip = "A white cane.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
            AddTrait("", "😎", "Prevents your 👁️ from reaching zero.", tattooIcon: "🦯", tattooScale: 0.7f, tattooOffset: new Vector2(7f, 6f));
        }
    }

    public override void OnWieldedBy(Thing thing)
    {
        base.OnWieldedBy(thing);

        thing.AdjustStatMin(StatType.Sight, 3);
        thing.AdjustStat(StatType.Attack, GetStatClamped(StatType.Attack));
        Trait = thing.AddTrait("", "😎", "Your 👁️ can't go down to zero.", tattooIcon: "🦯", tattooScale: 0.7f, tattooOffset: new Vector2(7f, 6f), source: DisplayName);
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        base.OnNoLongerWieldingThing(thing);

        thing.AdjustStatMin(StatType.Sight, -3);
        thing.AdjustStat(StatType.Attack, -GetStatClamped(StatType.Attack));
        thing.RemoveTrait(Trait);
    }
}
