using Sandbox;
using System;

namespace Roguemoji;
public partial class Axe : Thing
{
    public int DurabilityAmount { get; private set; }
    public int DurabilityCost { get; private set; }
    public int TreeAttackAmount { get; private set; }

    public Axe()
	{
		DisplayIcon = "🪓";
        DisplayName = "Axe";
        Description = "A powerful weapon, especially against trees";
        Tooltip = "An axe";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;

        if (Game.IsServer)
        {
            DurabilityAmount = 100;
            DurabilityCost = 1;
            TreeAttackAmount = 100;

            InitStat(StatType.Attack, 3);
            InitStat(StatType.Durability, current: DurabilityAmount, max: DurabilityAmount);
            AddTrait("", "🌲", $"Deals {TreeAttackAmount}{GetStatIcon(StatType.Attack)} against trees", offset: Vector2.Zero, tattooIcon: "🪓", tattooScale: 0.7f, tattooOffset: new Vector2(6f, 6f));
            //AddTrait("", "🌲", $"Deals {TreeAttackAmount}{GetStatIcon(StatType.Attack)} against trees", offset: Vector2.Zero, tattooIcon: "⚔️", tattooScale: 0.7f, tattooOffset: new Vector2(0f, 0f), labelText: $"{TreeAttackAmount}", labelOffset: new Vector2(0f, 0f), labelFontSize: 15, labelColor: Color.White);
        }
    }

    public override void OnWieldedBy(Thing thing)
    {
        base.OnWieldedBy(thing);

        thing.AdjustStat(StatType.Attack, GetStatClamped(StatType.Attack));
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        base.OnNoLongerWieldingThing(thing);

        thing.AdjustStat(StatType.Attack, -GetStatClamped(StatType.Attack));
    }

    public override void HitOther(Thing target, Direction direction)
    {
        if (target is TreeDeciduous || target is TreeEvergreen)
        {
            target.VfxShake(0.25f, 6f);
            target.Hurt(TreeAttackAmount);
        }
        else
        {
            base.HitOther(target, direction);
        }

        AdjustStat(StatType.Durability, -DurabilityCost);
        if (GetStatClamped(StatType.Durability) == 0)
            Destroy();
    }
}
