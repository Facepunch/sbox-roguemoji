using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionMana : Potion
{
    public override string SplashIcon => GetStatIcon(StatType.Mana);
    public int ManaAmount { get; private set; }

    public PotionMana()
    {
        PotionType = PotionType.Mana;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Recover some mana";
        Tooltip = "A mana potion";

        SetTattoo(GetStatIcon(StatType.Mana));

        if (Game.IsServer)
        {
            ManaAmount = 5;
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
        ApplyEffectToThing(user);
        Destroy();

        base.Use(user);
    }

    public override void ApplyEffectToThing(Thing thing)
    {
        if (!thing.HasStat(StatType.Mana))
            return;

        int amountRecovered = Math.Min(ManaAmount, thing.GetStatMax(StatType.Mana) - thing.GetStatClamped(StatType.Mana));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Mana), thing.GridPos, 1.33f, thing.CurrentLevelId, new Vector2(Game.Random.Float(8f, 12f) * (thing.FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (thing.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, parent: thing);
        thing.AdjustStat(StatType.Mana, ManaAmount);
    }
}
