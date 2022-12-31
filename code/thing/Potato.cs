using Sandbox;
using System;

namespace Roguemoji;
public partial class Potato : Thing
{
    public int EatHealth { get; set; }

	public Potato()
	{
		DisplayIcon = "🥔";
        DisplayName = "Potato";
        Description = "Uncooked and as hard as a rock.";
        Tooltip = "A potato.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable;
        EatHealth = 2;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
            AddTrait("", "🍽️", $"Eat for +{EatHealth}❤️", tattooIcon: "❤️", tattooScale: 0.7f, tattooOffset: new Vector2(0f, 2f), labelText: $"+{EatHealth}", labelFontSize: 18, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        user.AdjustStat(StatType.Health, EatHealth);
        Destroy();
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
