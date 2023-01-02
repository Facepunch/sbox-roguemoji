using Sandbox;
using System;

namespace Roguemoji;
public partial class Nut : Thing
{
    public int EatHealth { get; set; }

    public Nut()
	{
		DisplayIcon = "🌰";
        DisplayName = "Nut";
        Description = "Some sort of nut.";
        Tooltip = "A nut.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable;
        EatHealth = 1;

        if (Game.IsServer)
        {
            AddTrait("", "🍽️", $"Eat for +{EatHealth}❤️", offset: Vector2.Zero, tattooIcon: "❤️", tattooScale: 0.8f, tattooOffset: new Vector2(0f, 2f), labelText: $"+{EatHealth}", labelFontSize: 18, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        user.AdjustStat(StatType.Health, EatHealth);
        Destroy();
    }
}
