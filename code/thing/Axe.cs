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
        Flammability = 13;

        if (Game.IsServer)
        {
            DurabilityAmount = 100;
            DurabilityCost = 1;
            TreeAttackAmount = 100;

            InitStat(StatType.Attack, 3);
            InitStat(StatType.Durability, current: DurabilityAmount, max: DurabilityAmount);
            AddTrait("", "🌲", $"Deals {TreeAttackAmount}{GetStatIcon(StatType.Attack)} to trees", offset: Vector2.Zero, tattooIcon: "🪓", tattooScale: 0.7f, tattooOffset: new Vector2(6f, 6f));
            AddTrait("", GetStatIcon(StatType.Attack), $"Attacking costs {DurabilityCost}{GetStatIcon(StatType.Durability)}", offset: new Vector2(0f, -3f), tattooIcon: GetStatIcon(StatType.Durability), tattooScale: 0.8f, tattooOffset: new Vector2(0f, 0f), labelText: $"-{DurabilityCost}", labelFontSize: 18, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));;
        }
    }

    public override void HitOther(Thing target, Direction direction)
    {
        if (target is TreeDeciduous || target is TreeEvergreen)
        {
            target.VfxShake(0.25f, 6f);
            target.ContainingGridManager.PlaySfx("tree_hit_by_axe", target.GridPos, loudness: 3);
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
