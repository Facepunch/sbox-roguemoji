using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CBlinded : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }
    public float AdjustTimer { get; set; }
    public float AdjustDelay { get; set; }

    public int CurrSightDelta { get; set; }
    public int TotalAmount { get; set; }
    public bool IsReducing { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        CurrSightDelta = 0;
        AdjustTimer = 0f;
        AdjustDelay = 0.075f;
        TotalAmount = Thing.GetStat(StatType.SightDistance).CurrentValue + 3;
        IsReducing = true;

        AdjustSight(-1);
        Trait = thing.AddTrait("Blinded", Globals.Icon(IconType.Blindness), $"Drastically reduced {Thing.GetStatIcon(StatType.SightDistance)}", offset: Vector2.Zero);

        thing.AddFloater(Globals.Icon(IconType.Blindness), time: 0f, new Vector2(-14f, -4f), Vector2.Zero, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.025f, scale: 0.4f, opacity: 0.35f);

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component))
            IconId = ((CIconPriority)component).AddIconPriority("😑", (int)PlayerIconPriority.Blinded);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;
        if(Lifetime > 0f && TimeElapsed > Lifetime)
        {
            Remove();
            return;
        }

        Trait.BarPercent = 1f - Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f);

        if(IsReducing)
        {
            if (CurrSightDelta > -TotalAmount)
            {
                AdjustTimer += dt;
                if (AdjustTimer > AdjustDelay)
                {
                    AdjustTimer -= AdjustDelay;
                    AdjustSight(-1);
                }
            }
            else
            {
                IsReducing = false;
                AdjustTimer = AdjustDelay;
            }
        }
        else
        {
            if(CurrSightDelta < 0)
            {
                if (TimeElapsed > (Lifetime - (TotalAmount + 10) * AdjustDelay))
                {
                    AdjustTimer += dt;
                    if (AdjustTimer > AdjustDelay)
                    {
                        AdjustTimer -= AdjustDelay;
                        AdjustSight(1);
                    }
                }
            }
        }
    }

    void AdjustSight(int amount)
    {
        Thing.AdjustStat(StatType.SightDistance, amount);
        CurrSightDelta += amount;
    }

    public override void OnRemove()
    {
        Thing.AdjustStat(StatType.SightDistance, -CurrSightDelta);
        Thing.RemoveTrait(Trait);
        Thing.RemoveFloater(Globals.Icon(IconType.Blindness));

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);
    }

    public override void OnThingDestroyed()
    {
        Thing.RemoveFloater(Globals.Icon(IconType.Blindness));
    }

    public override void OnThingDied()
    {
        Remove();
    }
}