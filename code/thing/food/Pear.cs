using Sandbox;
using System;

namespace Roguemoji;
public partial class Pear : Thing
{
    public int EatHealth { get; set; }
    public override string AbilityName => "Eat";

    public Pear()
    {
        DisplayIcon = "🍐";
        DisplayName = "Pear";
        Description = "Pear-shaped and delicious";
        Tooltip = "A pear";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        if (Game.IsServer)
        {
            EatHealth = 2;
            AddTrait(AbilityName, "🍽️", $"Consume for +{EatHealth}{GetStatIcon(StatType.Health)}", offset: Vector2.Zero, tattooIcon: GetStatIcon(StatType.Health), tattooScale: 0.8f, tattooOffset: new Vector2(0f, 2f), labelText: $"+{EatHealth}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f), isAbility: true);
        }
    }

    public override void Use(Thing user)
    {
        int amountRecovered = Math.Min(EatHealth, user.GetStatMax(StatType.Health) - user.GetStatClamped(StatType.Health));
        user.AddFloater(GetStatIcon(StatType.Health), 1.33f, new Vector2(Game.Random.Float(8f, 12f) * (user.FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (user.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f);
        user.AdjustStat(StatType.Health, EatHealth);
        Destroy();

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😋", (int)PlayerIconPriority.EatReaction, 1.0f);

        base.Use(user);
    }
}
