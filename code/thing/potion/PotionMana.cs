using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionMana : Potion
{
    public override string ChatDisplayIcons => $"🧉{GetStatIcon(StatType.Mana)}";
    public override string AbilityName => "Quaff Potion";
    public int ManaAmount { get; private set; }

    public PotionMana()
    {
        PotionType = PotionType.Mana;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = "Mana Potion";
        Description = "Recover some mana";
        Tooltip = "A mana potion";

        SetTattoo(GetStatIcon(StatType.Mana));

        if (Game.IsServer)
        {
            ManaAmount = 5;
            AddTrait(AbilityName, "😋", $"Consume potion to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f));
            AddTrait("", GetStatIcon(StatType.Mana), $"Drinking recovers {ManaAmount}{GetStatIcon(StatType.Mana)}", offset: new Vector2(0f, -3f), labelText: $"+{ManaAmount}", labelFontSize: 16, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (!user.HasStat(StatType.Mana))
            return false;

        return true;
    }

    public override void Use(Thing user)
    {
        int amountRecovered = Math.Min(ManaAmount, user.GetStatMax(StatType.Mana) - user.GetStatClamped(StatType.Mana));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Mana), user.GridPos, 1.33f, user.CurrentLevelId, new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), new Vector2(Game.Random.Float(-10f, 10f), Game.Random.Float(0f, -10f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, parent: user);
        user.AdjustStat(StatType.Mana, ManaAmount);

        Destroy();

        base.Use(user);
    }
}
