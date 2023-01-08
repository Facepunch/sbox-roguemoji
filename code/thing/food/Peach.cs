using Roguemoji;
using Sandbox;
using System;

namespace Roguemoji;
public partial class Peach : Thing
{
    public int EatCharisma { get; set; }
    public override string AbilityName => "Eat";

    public Peach()
    {
        DisplayIcon = "🍑";
        DisplayName = "Peach";
        Description = "Sweet and juicy";
        Tooltip = "A peach";
        IconDepth = 0;
        ShouldLogBehaviour = true;
        Flags = ThingFlags.Selectable | ThingFlags.Useable;

        if (Game.IsServer)
        {
            EatCharisma = 1;
            AddTrait(AbilityName, "🍽️", $"Consume for +{EatCharisma}{GetStatIcon(StatType.Charisma)}", offset: Vector2.Zero, tattooIcon: GetStatIcon(StatType.Charisma), tattooScale: 0.8f, tattooOffset: new Vector2(0f, 1f), labelText: $"+{EatCharisma}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        int amountGained = Math.Min(EatCharisma, user.GetStatMax(StatType.Charisma) - user.GetStatClamped(StatType.Charisma));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Charisma), user.GridPos, 1.2f, user.CurrentLevelId, new Vector2(0f, 1f), new Vector2(0f, -6f), $"+{amountGained}", requireSight: true, EasingType.SineOut, 0.25f, parent: user);

        user.AdjustStat(StatType.Charisma, EatCharisma);
        Destroy();

        base.Use(user);
    }
}
