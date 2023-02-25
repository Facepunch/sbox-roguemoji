using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollHeal : Scroll
{
    public ScrollHeal()
    {
        ScrollType = ScrollType.Heal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Target recovers some health";
        Tooltip = "A scroll of Heal";

        SetTattoo(Globals.Icon(IconType.Heal));

        if (Game.IsServer)
        {
            //AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😘", (int)PlayerIconPriority.UseScroll, 1.0f);

        var targetThing = user.ContainingGridManager.GetThingsAt(targetGridPos).Where(x => x.HasStat(StatType.Health)).OrderByDescending(x => x.GetZPos()).FirstOrDefault();
        if (targetThing == null)
        {
            user.ContainingGridManager.AddFloater($"{Globals.Icon(IconType.Heal)}", targetGridPos, 0.85f, new Vector2(0, 3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f, opacity: 0.5f);
            Destroy();
            return;
        }

        int healthAmount = 7;
        int amountRecovered = Math.Min(healthAmount, targetThing.GetStatMax(StatType.Health) - targetThing.GetStatClamped(StatType.Health));
        targetThing.AddFloater(GetStatIcon(StatType.Health), 1.33f, new Vector2(Game.Random.Float(8f, 12f) * (targetThing.FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (targetThing.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f);
        targetThing.AdjustStat(StatType.Health, healthAmount);

        targetThing.AddFloater($"{Globals.Icon(IconType.Heal)}", 0.75f, new Vector2(0, 3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f, opacity: 0.5f);

        Destroy();
    }
}
