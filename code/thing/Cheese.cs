using Sandbox;
using System;

namespace Roguemoji;
public partial class Cheese : Thing
{
    public int EatHealth { get; set; }
    public override string AbilityName => "Eat";

    public Cheese()
	{
		DisplayIcon = "🧀";
        DisplayName = "Cheese";
        Tooltip = "A wedge of cheese";
        Description = "Stinky and delicious";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable;

        if (Game.IsServer)
        {
            EatHealth = 2;
            AddTrait(AbilityName, "🍽️", $"Consume for +{EatHealth}{GetStatIcon(StatType.Health)}", offset: Vector2.Zero, tattooIcon: GetStatIcon(StatType.Health), tattooScale: 0.8f, tattooOffset: new Vector2(0f, 2f), labelText: $"+{EatHealth}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        user.AdjustStat(StatType.Health, EatHealth);
        Destroy();

        base.Use(user);
    }
}
