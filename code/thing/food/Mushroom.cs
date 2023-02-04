using Sandbox;
using System;

namespace Roguemoji;
public partial class Mushroom : Thing
{
    public override string AbilityName => "Eat";

    public Mushroom()
    {
        DisplayIcon = "🍄";
        DisplayName = "Mushroom";
        Description = "There's a good chance it's poisonous";
        Tooltip = "A mushroom";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "🍽️", $"Consume for a random positive or negative effect", offset: Vector2.Zero, tattooIcon: "🍄", tattooScale: 0.75f, tattooOffset: new Vector2(-0.3f, 0.1f), labelText: "❓️", labelOffset: new Vector2(0f, -16f), labelFontSize: 9, labelColor: Color.White, isAbility: true);
        }
    }

    public override void Use(Thing user)
    {
        int rand = Game.Random.Int(0, 1);
        switch (rand)
        {
            case 0:
                int amount = 8;
                int amountRecovered = Math.Min(amount, user.GetStatMax(StatType.Health) - user.GetStatClamped(StatType.Health));
                user.AddSideFloater(GetStatIcon(StatType.Health), text: $"+{amountRecovered}");
                user.AdjustStat(StatType.Health, amount);

                if (user is Smiley && user.GetComponent<CIconPriority>(out var c))
                    ((CIconPriority)c).AddIconPriority("😋", (int)PlayerIconPriority.EatReaction, 0.5f);

                break;
            case 1:
                var poison = user.AddComponent<CPoisoned>();
                poison.Lifetime = 60f;
                user.AddSideFloater(Globals.Icon(IconType.Poison));

                if (user is Smiley && user.GetComponent<CIconPriority>(out var c2))
                    ((CIconPriority)c2).AddIconPriority("🤢", (int)PlayerIconPriority.EatReaction, 0.5f);

                break;
        }

        Destroy();

        base.Use(user);
    }
}
