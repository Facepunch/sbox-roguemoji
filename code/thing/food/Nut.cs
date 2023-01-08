using Roguemoji;
using Sandbox;
using System;

namespace Roguemoji;
public partial class Nut : Thing
{
    public int EatHealth { get; set; }
    public override string AbilityName => "Eat";

    public Nut()
    {
        DisplayIcon = "🌰";
        DisplayName = "Nut";
        Description = "Some sort of nut";
        Tooltip = "A nut";
        IconDepth = 0;
        Flags = ThingFlags.Selectable | ThingFlags.Useable;

        if (Game.IsServer)
        {
            EatHealth = 1;
            AddTrait(AbilityName, "🍽️", $"Consume for +{EatHealth}{GetStatIcon(StatType.Health)}", offset: Vector2.Zero, tattooIcon: GetStatIcon(StatType.Health), tattooScale: 0.8f, tattooOffset: new Vector2(0f, 2f), labelText: $"+{EatHealth}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
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
}
