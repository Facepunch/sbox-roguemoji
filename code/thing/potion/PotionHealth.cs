using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionHealth : Potion
{
    public override string ChatDisplayIcons => $"🧉{GetStatIcon(StatType.Health)}";
    public override string AbilityName => "Quaff Potion";
    public int HealthAmount { get; private set; }

    public PotionHealth()
    {
        PotionType = PotionType.Health;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = "Health Potion";
        Description = "Recover some health";
        Tooltip = "A health potion";
        
        SetTattoo(GetStatIcon(StatType.Health));

        if (Game.IsServer)
        {
            HealthAmount = 5;
            AddTrait(AbilityName, "😋", $"Consume potion to cause an effect", offset: new Vector2(0f, -1f), tattooIcon: "🧉", tattooScale: 0.5f, tattooOffset: new Vector2(-8f, 8f));
            AddTrait("", GetStatIcon(StatType.Health), $"Drinking recovers {HealthAmount}{GetStatIcon(StatType.Health)}", offset: new Vector2(0f, -1f), labelText: $"+{HealthAmount}", labelFontSize: 16, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (!user.HasStat(StatType.Health))
            return false;

        return true;
    }

    public override void Use(Thing user)
    {
        int amountRecovered = Math.Min(HealthAmount, user.GetStatMax(StatType.Health) - user.GetStatClamped(StatType.Health));
        RoguemojiGame.Instance.AddFloater(GetStatIcon(StatType.Health), user.GridPos, 1.33f, user.CurrentLevelId, new Vector2(Game.Random.Float(8f, 12f) * (user.FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (user.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, parent: user);
        user.AdjustStat(StatType.Health, HealthAmount);

        Destroy();

        base.Use(user);
    }
}
