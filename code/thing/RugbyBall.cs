using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class RugbyBall : Thing
{
    [Net] public int EnergyCost { get; private set; }
    public float CooldownTime { get; private set; }

    public override string AbilityName => "Charge";

    public RugbyBall()
	{
		DisplayIcon = "🏉";
        DisplayName = "Rugby Ball";
        Description = "Charge with this and knock things back";
        Tooltip = "A rugby ball";
        IconDepth = 0;
		Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming;

        if (Game.IsServer)
        {
            EnergyCost = 6;
            CooldownTime = 15f;

            AddTrait(AbilityName, "💪", $"Run in a direction and bodyslam", offset: new Vector2(0f, -1f), tattooIcon: "🏉", tattooOffset: new Vector2(1f, -1.5f), tattooScale: 0.55f, isAbility: true);
            AddTrait("", GetStatIcon(StatType.Energy), $"Ability costs {EnergyCost}{GetStatIcon(StatType.Energy)}", offset: new Vector2(0f, -3f), labelText: $"{EnergyCost}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
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

    public override void Use(Thing user, Direction direction)
    {
        if (IsOnCooldown)
            return;

        if (!user.TrySpendStat(StatType.Energy, EnergyCost))
            return;

        var charge = user.AddComponent<CRugbyCharge>();
        charge.Direction = direction;
        charge.MoveDelay = 0.1f;
        charge.RemainingDistance = 10;

        StartCooldown(CooldownTime);

        base.Use(user, direction);
    }
}

public class CRugbyCharge : ThingComponent
{
    public Direction Direction { get; set; }
    public float MoveDelay { get; set; }
    public TimeSince TimeSinceMove { get; set; }
    public int RemainingDistance { get; set; }
    public int DistanceMoved { get; set; }
    public float StartTimer { get; set; }
    public float StartDelay { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        TimeSinceMove = 0f;
        DistanceMoved = 0;
        StartDelay = 0.5f;

        if (thing.GetComponent<CActing>(out var component))
            ((CActing)component).PreventAction();

        if (thing is RoguemojiPlayer && thing.GetComponent<CIconPriority>(out var component2))
            IconId = ((CIconPriority)component2).AddIconPriority("😤", (int)PlayerIconPriority.RugbyCharge);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (Thing.ContainingGridType != GridType.Arena)
        {
            Remove();
            return;
        }

        if(StartTimer < StartDelay)
        {
            StartTimer += dt;
            return;
        }

        if (TimeSinceMove > MoveDelay)
        {
            TimeSinceMove = 0f;

            var oldPos = Thing.GridPos;

            if (Thing.TryMove(Direction, dontRequireAction: true))
            {
                RemainingDistance--;
                if (RemainingDistance <= 0)
                    Remove();

                DistanceMoved++;

                RoguemojiGame.Instance.AddFloater("☁️", oldPos, 0.5f, Thing.CurrentLevelId, new Vector2(0f, 4f), new Vector2(0f, -7f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.SineOut, fadeInTime: 0.15f, scale: 1.1f, opacity: 0.5f, parent: null);
            }
        }

        //Thing.DebugText = $"{RemainingDistance}";
    }

    public override void OnBumpedIntoThing(Thing thing)
    {
        ShoveThing(thing);

        Remove();
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        Thing.HitOther(thing, Direction);
        ShoveThing(thing);
        
        Remove();
    }

    void ShoveThing(Thing thing)
    {
        var oldPos = thing.GridPos;
        if(thing.TryMove(Direction, dontRequireAction: true))
        {
            RoguemojiGame.Instance.AddFloater("☁️", oldPos, 0.5f, Thing.CurrentLevelId, new Vector2(0f, 4f), new Vector2(0f, -7f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.SineOut, fadeInTime: 0.15f, scale: 1.1f, opacity: 0.5f, parent: null);

            if (thing.GetComponent<CActing>(out var component))
            {
                var acting = (CActing)component;
                acting.ActionTimer = 0f;
            }
        }
    }

    public override void OnBumpedOutOfBounds(Direction dir)
    {
        Remove();
    }

    public override void OnRemove()
    {
        base.OnRemove();

        if (Thing.GetComponent<CActing>(out var component))
            ((CActing)component).AllowAction();

        if (Thing is RoguemojiPlayer && Thing.GetComponent<CIconPriority>(out var component2))
            ((CIconPriority)component2).RemoveIconPriority(IconId);
    }

    public override void OnThingDied()
    {
        Remove();
    }
}