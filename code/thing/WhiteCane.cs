using Sandbox;
using System;

namespace Roguemoji;
public partial class WhiteCane : Thing
{
    public int MinSightChange { get; set; }
    public Trait Trait { get; private set; }

	public WhiteCane()
	{
		DisplayIcon = "🦯";
        DisplayName = "White Cane";
        Description = "Useful when you can't see anything";
        Tooltip = "A white cane";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;

        if (Game.IsServer)
        {
            MinSightChange = 2;

            InitStat(StatType.Attack, 1);
            AddTrait("", "😎", $"Prevents your {GetStatIcon(StatType.Sight)} from reaching zero", offset: Vector2.Zero, tattooIcon: "🦯", tattooScale: 0.7f, tattooOffset: new Vector2(7f, 6f));
        }
    }

    public override void OnWieldedBy(Thing thing)
    {
        base.OnWieldedBy(thing);

        thing.AdjustStatMin(StatType.Sight, MinSightChange);
        thing.AdjustStat(StatType.Attack, GetStatClamped(StatType.Attack));
        Trait = thing.AddTrait("", "😎", $"Your {GetStatIcon(StatType.Sight)} can't go down to zero", offset: Vector2.Zero, tattooIcon: "🦯", tattooScale: 0.7f, tattooOffset: new Vector2(7f, 6f), source: DisplayName);
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        base.OnNoLongerWieldingThing(thing);

        thing.AdjustStatMin(StatType.Sight, -MinSightChange);
        thing.AdjustStat(StatType.Attack, -GetStatClamped(StatType.Attack));
        thing.RemoveTrait(Trait);
    }
}
