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
        IconDepth = 0;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        if (Game.IsServer)
        {
            EatEnergy = 5;
            AddTrait(AbilityName, "🍽️", $"Consume for +{EatEnergy}{GetStatIcon(StatType.Energy)}", offset: Vector2.Zero, tattooIcon: GetStatIcon(StatType.Energy), tattooScale: 0.8f, tattooOffset: new Vector2(0f, 0f), labelText: $"+{EatEnergy}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        int amountRecovered = Math.Min(EatEnergy, user.GetStatMax(StatType.Energy) - user.GetStatClamped(StatType.Energy));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Energy), GridPos, 1.33f, CurrentLevelId, new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), new Vector2(Game.Random.Float(-10f, 10f), Game.Random.Float(0f, -10f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, parent: user);
        user.AdjustStat(StatType.Energy, EatEnergy);
        Destroy();

        base.Use(user);
    }
}
