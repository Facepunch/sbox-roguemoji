using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class Cigarette : Thing
{
    [Net] public int EnergyCost { get; private set; }
    public float CooldownTime { get; private set; }
    public int DurabilityAmount { get; private set; }
    public int DurabilityCost { get; private set; }

    public override string AbilityName => "Take A Drag";

    public Cigarette()
	{
		DisplayIcon = "🚬";
        DisplayName = "Cigarette";
        Description = "Unhealthy but cool";
        Tooltip = "A cigarette";
        IconDepth = 0;
		Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        if (Game.IsServer)
        {
            EnergyCost = 4;
            CooldownTime = 1f;
            DurabilityAmount = 5;
            DurabilityCost = 1;

            InitStat(StatType.Durability, current: DurabilityAmount, max: DurabilityAmount);

            AddTrait(AbilityName, "💨", "Puff the cigarette, increasing energy and exhaling a cloud of smoke nearby", offset: new Vector2(0f, -1f));
            AddTrait("", GetStatIcon(StatType.Durability), $"Ability uses {DurabilityCost}{GetStatIcon(StatType.Durability)}", offset: new Vector2(0f, -3f), labelText: $"{DurabilityCost}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "⏳", $"Cooldown time: {CooldownTime}s", offset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        var energy = user.GetStatClamped(StatType.Energy);
        if (energy < EnergyCost && !ignoreResources)
        {
            if (shouldLogMessage && user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {EnergyCost}{GetStatIcon(StatType.Energy)} to use {ChatDisplayIcons} but you only have {energy}{GetStatIcon(StatType.Energy)}");

            return false;
        }

        return true;
    }

    public override void Use(Thing user)
    {
        if (IsOnCooldown)
            return;

        if (!this.TrySpendStat(StatType.Durability, 1))
            return;

        var potentialDirections = user.ContainingGridManager.GetDirectionsInBounds(user.GridPos, cardinalOnly: false);
        var direction = potentialDirections[Game.Random.Int(0, potentialDirections.Count - 1)];

        var smoke = user.ContainingGridManager.SpawnThing<ProjectileCigaretteSmoke>(user.GridPos + GridManager.GetIntVectorForDirection(direction));
        smoke.SetTransformClient(degrees: GridManager.GetDegreesForDirection(direction) + 90f);
        smoke.Direction = direction;

        var projectile = smoke.AddComponent<CProjectile>();
        projectile.Direction = direction;
        projectile.MoveDelay = 0.2f;
        projectile.RemainingDistance = 2;
        projectile.Thrower = user;
        projectile.ShouldHit = false;

        base.Use(user, direction);

        if (GetStatClamped(StatType.Durability) == 0)
            Destroy();
        else
            StartCooldown(CooldownTime);
    }
}
