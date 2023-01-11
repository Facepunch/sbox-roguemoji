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
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        if (Game.IsServer)
        {
            EatCharisma = 1;
            AddTrait(AbilityName, "🍽️", $"Consume for +{EatCharisma}{GetStatIcon(StatType.Charisma)}", offset: Vector2.Zero, tattooIcon: GetStatIcon(StatType.Charisma), tattooScale: 0.8f, tattooOffset: new Vector2(0f, 1f), labelText: $"+{EatCharisma}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        int amountGained = Math.Min(EatCharisma, user.GetStatMax(StatType.Charisma) - user.GetStatClamped(StatType.Charisma));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Charisma), GridPos, 1.33f, CurrentLevelId, new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), new Vector2(Game.Random.Float(-10f, 10f), Game.Random.Float(0f, -10f)), height: Game.Random.Float(10f, 35f), text: $"+{amountGained}", requireSight: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.85f, parent: user);
        user.AdjustStat(StatType.Charisma, EatCharisma);
        Destroy();

        base.Use(user);
    }
}
