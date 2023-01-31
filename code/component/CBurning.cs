﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public class CBurning : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }
    public int IconId { get; set; }
    public float BurnCountdown { get; set; }
    public float BurnDelayMin { get; set; }
    public float BurnDelayMax { get; set; }
    public int BurnDamage { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        BurnDelayMin = 2f;
        BurnDelayMax = 3f;
        BurnCountdown = Game.Random.Float(BurnDelayMin, BurnDelayMax);
        BurnDamage = 1;

        Trait = thing.AddTrait("Burning", Globals.Icon(IconType.Burning), $"Being consumed by fire", offset: Vector2.Zero);

        if (thing is RoguemojiPlayer && thing.GetComponent<CIconPriority>(out var component2))
            IconId = ((CIconPriority)component2).AddIconPriority("🥵", (int)PlayerIconPriority.Sleeping);

        thing.AddFloater(Globals.Icon(IconType.Burning), time: 0f, new Vector2(0f, -12f), Vector2.Zero, height: 0f, text: "", requireSight: true, 
            alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.25f, scale: 1f, opacity: 0.5f, shakeAmount: 1f, showOnInvisible: true);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        TimeElapsed += dt;
        if(Lifetime > 0f && TimeElapsed > Lifetime)
        {
            Remove();

            if (!Thing.HasStat(StatType.Health))
            {
                Thing.ContainingGridManager.AddFloater(Globals.Icon(IconType.Burning), Thing.GridPos, 0.5f, new Vector2(0f, -12f), new Vector2(0, -18f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadIn, fadeInTime: 0.04f, scale: 1f, opacity: 0.6f, shakeAmount: 1f);
                Thing.Destroy();
            }

            return;
        }

        Trait.BarPercent = 1f - Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f);

        BurnCountdown -= dt;
        if(BurnCountdown < 0f)
        {
            Burn();
            BurnCountdown = Game.Random.Float(BurnDelayMin, BurnDelayMax);
        }
    }

    void Burn()
    {
        if (Thing.HasStat(StatType.Health) && Thing.GetStatClamped(StatType.Health) > 0)
        {
            Thing.Hurt(BurnDamage, showImpactFloater: false);
        }

        for(int x = -1; x <= 1; x++) 
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                var offset = new IntVector(x, y);
                if (offset.ManhattanLength > 1)
                    continue;

                var gridPos = Thing.GridPos + offset;

                if (Thing.ContainingGridManager == null || !Thing.ContainingGridManager.IsGridPosInBounds(gridPos))
                    continue;

                var otherThings = Thing.ContainingGridManager.GetThingsAt(gridPos).ToList();

                if(otherThings.Count == 0)
                {
                    var startOffset = -offset * 40f;
                    var endOffset = new Vector2(0f, 0f);
                    var height = Game.Random.Float(10f, 20f);

                    Thing.ContainingGridManager.AddFloater(Globals.Icon(IconType.Burning), gridPos, 0.15f, startOffset, endOffset, height: height, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.01f, scale: Game.Random.Float(0.6f, 0.75f), opacity: 0.5f, shakeAmount: 1f);
                }
                else
                {
                    foreach (var other in otherThings)
                    {
                        if(!other.HasComponent<CBurning>())
                        {
                            var burning = other.AddComponent<CBurning>();
                            burning.Lifetime = Math.Max(Lifetime, burning.Lifetime);
                        }
                    }
                }
            }
        }
    }

    public override void OnRemove()
    {
        Thing.RemoveTrait(Trait);

        if (Thing is RoguemojiPlayer && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);

        Thing.RemoveFloater(Globals.Icon(IconType.Burning));
    }

    public override void OnThingDied()
    {
        Remove();
    }
}