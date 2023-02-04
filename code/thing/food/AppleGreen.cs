using Sandbox;
using System;

namespace Roguemoji;
public partial class AppleGreen : Thing
{
    public int EatEnergy { get; set; }
    public override string AbilityName => "Eat";

    public AppleGreen()
    {
        DisplayIcon = "🍏";
        DisplayName = "Green Apple";
        Description = "Full of energy";
        Tooltip = "A green apple";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        if (Game.IsServer)
        {
            EatEnergy = 5;
            AddTrait(AbilityName, "🍽️", $"Consume for +{EatEnergy}{GetStatIcon(StatType.Energy)}", offset: Vector2.Zero, tattooIcon: GetStatIcon(StatType.Energy), tattooScale: 0.8f, tattooOffset: new Vector2(0f, 0f), labelText: $"+{EatEnergy}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f), isAbility: true);
        }
    }

    public override void Use(Thing user)
    {
        int amountRecovered = Math.Min(EatEnergy, user.GetStatMax(StatType.Energy) - user.GetStatClamped(StatType.Energy));

        user.AddFloater(GetStatIcon(StatType.Energy), 1.33f, new Vector2(Game.Random.Float(8f, 12f) * (user.FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (user.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f);

        user.AdjustStat(StatType.Energy, EatEnergy);
        Destroy();

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😋", (int)PlayerIconPriority.EatReaction, 1.0f);

        base.Use(user);
    }
}
