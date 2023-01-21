using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class Cigarette : Thing
{
    public int EnergyGained { get; private set; }
    public float CooldownTime { get; private set; }
    public int DurabilityAmount { get; private set; }
    public int HealthCost { get; private set; }
    public int DurabilityCost { get; private set; }
    public int CharismaAmount { get; private set; }
    public float CharismaTime { get; private set; }


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
            EnergyGained = 10;
            CooldownTime = 1f;
            DurabilityAmount = 5;
            HealthCost = 1;
            DurabilityCost = 1;
            CharismaAmount = 2;
            CharismaTime = 30f;

            InitStat(StatType.Durability, current: DurabilityAmount, max: DurabilityAmount);

            AddTrait(AbilityName, "💨", $"Puff the cigarette, gaining {GetStatIcon(StatType.Energy)} and exhaling a cloud of smoke nearby", offset: new Vector2(0f, -1f), isAbility: true);
            AddTrait("", GetStatIcon(StatType.Health), $"Smoking removes {HealthCost}{GetStatIcon(StatType.Health)}", offset: new Vector2(0f, 0f), labelText: $"-{HealthCost}", labelFontSize: 18, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", GetStatIcon(StatType.Energy), $"You gain {EnergyGained}{GetStatIcon(StatType.Energy)} for taking a drag", offset: new Vector2(0f, 0f), labelText: $"+{EnergyGained}", labelFontSize: 18, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", GetStatIcon(StatType.Charisma), $"You gain {CharismaAmount}{GetStatIcon(StatType.Charisma)} for {CharismaTime}s after taking a drag", offset: new Vector2(0f, 0f), labelText: $"+{CharismaAmount}", labelFontSize: 18, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", GetStatIcon(StatType.Durability), $"Ability costs {DurabilityCost}{GetStatIcon(StatType.Durability)}", offset: new Vector2(0f, -3f), labelText: $"-{DurabilityCost}", labelFontSize: 18, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "⏳", $"Cooldown time: {CooldownTime}s", offset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 18, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user)
    {
        if (IsOnCooldown)
            return;

        AdjustStat(StatType.Durability, -DurabilityCost);

        // take damage
        user.TakeDamage(1);

        // temp charisma buff
        if(user.GetComponent<CCigaretteCharismaBuff>(out var component))
        {
            component.ReInitialize();
        }
        else
        {
            CCigaretteCharismaBuff buff = user.AddComponent<CCigaretteCharismaBuff>();
            buff.Lifetime = CharismaTime;
            buff.Change(StatType.Charisma, CharismaAmount);
        }

        // gain energy
        int amountRecovered = Math.Min(EnergyGained, user.GetStatMax(StatType.Energy) - user.GetStatClamped(StatType.Energy));
        user.AddSideFloater(GetStatIcon(StatType.Energy), offsetStart: new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), offsetEnd: new Vector2(Game.Random.Float(10f, 20f) * (user.FloaterNum++ % 2 == 0 ? -1 : 1)), text: $"+{amountRecovered}");
        user.AdjustStat(StatType.Energy, EnergyGained);

        // blow smoke
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

public class CCigaretteCharismaBuff : ThingComponent
{
    public float Lifetime { get; set; }
    public StatType StatType { get; set; }
    public int ChangeAmount { get; set; }
    public Trait Trait { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
    }

    public void Change(StatType statType, int amount)
    {
        StatType = statType;
        ChangeAmount = amount;

        Thing.AdjustStat(StatType, ChangeAmount);
        string changeSign = Math.Sign(ChangeAmount) > 0 ? "+" : "-";
        Trait = Thing.AddTrait("", Thing.GetStatIcon(StatType), $"Smoking gave you {changeSign}{ChangeAmount}{Thing.GetStatIcon(StatType)} temporarily", offset: new Vector2(0f, 0f), 
            tattooIcon: "🚬", tattooScale: 0.65f, tattooOffset: new Vector2(8f, 1f));

        RoguemojiGame.Instance.AddFloater(Thing.GetStatIcon(StatType), Thing.GridPos, 1.5f, Thing.CurrentLevelId, new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), new Vector2(Game.Random.Float(10f, 20f) * (Thing.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{ChangeAmount}", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.75f, scale: 0.75f, parent: Thing);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;

        Trait.BarPercent = 1f - Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f);

        if (Lifetime > 0f && TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.AdjustStat(StatType, -ChangeAmount);
        Thing.RemoveTrait(Trait);
        RoguemojiGame.Instance.AddFloater(Thing.GetStatIcon(StatType), Thing.GridPos, 1.5f, Thing.CurrentLevelId, new Vector2(Game.Random.Float(-7f, 7f), Game.Random.Float(-1f, 10f)), new Vector2(Game.Random.Float(10f, 20f) * (Thing.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"-{ChangeAmount}", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.75f, scale: 0.75f, parent: Thing);
    }
}