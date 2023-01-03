using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class BowAndArrow : Thing
{
    public int EnergyCost { get; private set; }
    public float CooldownTime { get; private set; }

    public override string AbilityName => "Shoot Arrow";

    public BowAndArrow()
	{
		DisplayIcon = "🏹";
        DisplayName = "Bow and Arrow";
        Description = "Shoots arrows.";
        Tooltip = "A bow and arrow.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable | ThingFlags.UseRequiresAiming;

        if (Game.IsServer)
        {
            EnergyCost = 3;
            CooldownTime = 5f;

            AddTrait(AbilityName, "🔰", "Shoot an arrow in any direction.", offset: new Vector2(0f, -1f));
            AddTrait("", GetStatIcon(StatType.Energy), $"{EnergyCost}{GetStatIcon(StatType.Energy)} used to perform ability.", offset: new Vector2(0f, -3f), labelText: $"{EnergyCost}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "⏳", $"Cooldown time: {CooldownTime}s", offset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool TryStartUsing(Thing user)
    {
        var energy = user.GetStatClamped(StatType.Energy);
        if (energy < EnergyCost)
        {
            if (user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {EnergyCost}{GetStatIcon(StatType.Energy)} to use {ChatDisplayIcons} but you only have {energy}{GetStatIcon(StatType.Energy)}");

            return false;
        }

        return true;
    }

    public override void Use(Thing user, Direction direction)
    {
        if (IsOnCooldown)
            return;

        if (!user.TrySpendStat(StatType.Energy, EnergyCost))
            return;

        var arrow = user.ContainingGridManager.SpawnThing<ProjectileArrow>(user.GridPos);
        arrow.SetTransformClient(degrees: GridManager.GetDegreesForDirection(direction));
        arrow.Direction = direction;

        var projectile = arrow.AddComponent<Projectile>();
        projectile.Direction = direction;
        projectile.MoveDelay = 0.1f;
        projectile.RemainingDistance = 8;

        StartCooldown(CooldownTime);

        base.Use(user, direction);
    }
}
