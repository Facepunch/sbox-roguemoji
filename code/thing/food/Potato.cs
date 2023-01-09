using Roguemoji;
using Sandbox;
using System;

namespace Roguemoji;
public partial class Potato : Thing
{
    public int EatHealth { get; set; }
    public override string AbilityName => "Eat";

    public Potato()
    {
        DisplayIcon = "🥔";
        DisplayName = "Potato";
        Description = "Uncooked and hard as a rock";
        Tooltip = "A potato";
        IconDepth = 0;

        if (Game.IsServer)
        {
            Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;
            EatHealth = 2;
            InitStat(StatType.Attack, 1);
            AddTrait(AbilityName, "🍽️", $"Consume for +{EatHealth}{GetStatIcon(StatType.Health)}", offset: Vector2.Zero, tattooIcon: GetStatIcon(StatType.Health), tattooScale: 0.7f, tattooOffset: new Vector2(0f, 2f), labelText: $"+{EatHealth}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        int amountRecovered = Math.Min(EatHealth, user.GetStatMax(StatType.Health) - user.GetStatClamped(StatType.Health));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Health), user.GridPos, 1.2f, user.CurrentLevelId, new Vector2(0f, 1f), new Vector2(0f, -6f), $"+{amountRecovered}", requireSight: true, EasingType.SineOut, 0.25f, parent: user);

        user.AdjustStat(StatType.Health, EatHealth);
        Destroy();

        base.Use(user);
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
}
