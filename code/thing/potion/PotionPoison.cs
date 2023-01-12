using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionPoison : Potion
{
    public override string ChatDisplayIcons => $"🧉{Globals.Icon(IconType.Poison)}";
    public override string AbilityName => "Quaff Potion";
    public int HealthAmount { get; private set; }

    public PotionPoison()
    {
        PotionType = PotionType.Poison;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = "Poison Potion";
        Description = "Poisons the drinker";
        Tooltip = "A poison potion";
        
        SetTattoo(Globals.Icon(IconType.Poison));

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
        var poison = user.AddComponent<CPoisoned>();
        poison.Lifetime = 60f;

        RoguemojiGame.Instance.AddFloater(Globals.Icon(IconType.Poison), user.GridPos, 1.33f, user.CurrentLevelId, new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), new Vector2(Game.Random.Float(-10f, 10f), Game.Random.Float(0f, -10f)), height: Game.Random.Float(10f, 35f), text: "", requireSight: true, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f, parent: user);

        Destroy();

        base.Use(user);
    }
}
