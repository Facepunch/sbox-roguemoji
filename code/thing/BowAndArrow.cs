﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class BowAndArrow : Thing
{
    public float CooldownTime { get; private set; }

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
            CooldownTime = 7f;
            AddTrait("", "", $"Cooldown time of {CooldownTime} seconds.", tattooIcon: "⏳", tattooScale: 1f, tattooOffset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 15, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user, Direction direction)
    {
        if (IsOnCooldown)
            return;

        base.Use(user, direction);

        var arrow = user.ContainingGridManager.SpawnThing<ProjectileArrow>(user.GridPos);
        arrow.SetTransformClient(degrees: GridManager.GetDegreesForDirection(direction));
        arrow.Direction = direction;

        var projectile = arrow.AddComponent<Projectile>();
        projectile.Direction = direction;
        projectile.MoveDelay = 0.1f;
        projectile.RemainingDistance = 8;

        StartCooldown(CooldownTime);
    }
}