using Sandbox;
using System;

namespace Roguemoji;
public partial class AppleRed : Thing
{
    public int EatHealth { get; set; }
    public override string AbilityName => "Eat";

    public AppleRed()
    {
        DisplayIcon = "🍎";
        DisplayName = "Red Apple";
        Description = "Keeps the 👨‍⚕️ away";
        Tooltip = "A red apple";
        IconDepth = 0;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        if (Game.IsServer)
        {
            EatHealth = 3;
            AddTrait(AbilityName, "🍽️", $"Consume for +{EatHealth}{GetStatIcon(StatType.Health)}", offset: Vector2.Zero, tattooIcon: GetStatIcon(StatType.Health), tattooScale: 0.8f, tattooOffset: new Vector2(0f, 2f), labelText: $"+{EatHealth}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        int amountRecovered = Math.Min(EatHealth, user.GetStatMax(StatType.Health) - user.GetStatClamped(StatType.Health));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Health), user.GridPos, 1.33f, user.CurrentLevelId, new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), new Vector2(Game.Random.Float(-10f, 10f), Game.Random.Float(0f, -10f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, parent: user);
        user.AdjustStat(StatType.Health, EatHealth);
        Destroy();

        base.Use(user);
    }
}
