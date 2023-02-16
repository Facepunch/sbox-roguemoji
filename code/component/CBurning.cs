using Sandbox;
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
    public float SpreadCountdown { get; set; }
    public float SpreadDelayMin { get; set; }
    public float SpreadDelayMax { get; set; }
    public int BurnDamage { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        thing.IgnitionAmount = Globals.IGNITION_MAX;

        ShouldUpdate = true;

        BurnDelayMin = 2f;
        BurnDelayMax = 3f;
        BurnCountdown = Game.Random.Float(BurnDelayMin, BurnDelayMax);
        BurnDamage = 1;

        SpreadDelayMin = 0.35f;
        SpreadDelayMax = 0.45f;
        SpreadCountdown = Game.Random.Float(SpreadDelayMin, SpreadDelayMax);

        Trait = thing.AddTrait("Burning", Globals.Icon(IconType.Burning), $"Being consumed by fire", offset: Vector2.Zero);

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component2))
            IconId = ((CIconPriority)component2).AddIconPriority("🥵", (int)PlayerIconPriority.Sleeping);

        thing.AddFloater(Globals.Icon(IconType.Burning), time: 0f, new Vector2(0f, -12f), new Vector2(0f, -12f), height: 0f, text: "", requireSight: true, 
            alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.25f, scale: 1f, opacity: 0.5f, shakeAmount: 1f, showOnInvisible: true);

        thing.ContainingGridManager.ThingFloaterCounter++;
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
            else
            {
                Thing.IgnitionAmount = 0;
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

        SpreadCountdown -= dt;
        if(SpreadCountdown < 0f)
        {
            Spread();
            SpreadCountdown = Game.Random.Float(SpreadDelayMin, SpreadDelayMax);
        }
    }

    void Burn()
    {
        if (Thing.HasStat(StatType.Health) && Thing.GetStatClamped(StatType.Health) > 0)
        {
            Thing.Hurt(BurnDamage, showImpactFloater: false);
        }
    }

    void Spread()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var offset = new IntVector(x, y);
                if (offset.ManhattanLength > 1)
                    continue;

                var gridPos = Thing.GridPos + offset;

                if (Thing.ContainingGridManager == null || !Thing.ContainingGridManager.IsGridPosInBounds(gridPos))
                    continue;

                bool shouldCellPutOutFire = Thing.ContainingGridManager.ShouldCellPutOutFire(gridPos);

                var otherThings = Thing.ContainingGridManager.GetThingsAt(gridPos).ToList();

                if (otherThings.Count == 0)
                {
                    if(!shouldCellPutOutFire && Game.Random.Int(0, 10) == 0 && offset.ManhattanLength > 0)
                    {
                        var startOffset = new Vector2(Game.Random.Float(-15f, 15f), Game.Random.Float(-15f, 15f));
                        var endOffset = startOffset + new Vector2(0f, Game.Random.Float(-15f, 0f));
                        var time = Game.Random.Float(0.25f, 0.35f);
                        var scale = Game.Random.Float(0.3f, 0.45f);
                        var opacity = Game.Random.Float(0.25f, 0.5f);
                        var shakeAmount = Game.Random.Float(0.1f, 0.4f);
                        var fadeInTime = Game.Random.Float(0.015f, 0.2f);

                        Thing.ContainingGridManager.AddFloater(Globals.Icon(IconType.Burning), gridPos, time, startOffset, endOffset, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: fadeInTime, scale: scale, opacity: opacity, shakeAmount: shakeAmount);
                    }
                }
                else
                {
                    bool hasMadeFloater = false;
                    foreach (var other in otherThings)
                    {
                        if (other == Thing)
                            continue;

                        if (!shouldCellPutOutFire && other.Flammability > 0 && !other.HasComponent<CBurning>())
                        {
                            float ADJUST_SPREAD_SPEED = 1.333f;
                            float proximityFactor = (offset.ManhattanLength == 0) ? 1.4f : 1f;
                            other.IgnitionAmount += MathX.FloorToInt(other.Flammability * proximityFactor * ADJUST_SPREAD_SPEED);
                            if (other.IgnitionAmount >= Globals.IGNITION_MAX)
                            {
                                var burning = other.AddComponent<CBurning>();
                                burning.Lifetime = Math.Max(Lifetime, burning.Lifetime);

                                other.IgnitionAmount = Globals.IGNITION_MAX;
                            }
                            else if(!hasMadeFloater)
                            {
                                var startOffset = new Vector2(Game.Random.Float(-15f, 15f), Game.Random.Float(-15f, 15f));
                                var endOffset = startOffset + new Vector2(0f, Game.Random.Float(-15f, 0f));
                                var time = Utils.Map(other.IgnitionAmount, 0, Globals.IGNITION_MAX, 0.25f, 1.25f, EasingType.Linear) * Game.Random.Float(0.9f, 1.2f);
                                var scale = Utils.Map(other.IgnitionAmount, 0, Globals.IGNITION_MAX, 0.4f, 0.75f, EasingType.QuadIn);
                                var opacity = Utils.Map(other.IgnitionAmount, 0, Globals.IGNITION_MAX, 0.33f, 1f, EasingType.QuadIn);
                                var shakeAmount = Utils.Map(other.IgnitionAmount, 0, Globals.IGNITION_MAX, 0.1f, 1f, EasingType.Linear);

                                other.AddFloater(Globals.Icon(IconType.Burning), time, startOffset, endOffset, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.025f, scale: scale, opacity: opacity, shakeAmount: shakeAmount);
                                hasMadeFloater = true;
                            }
                        }
                        else if(other is PuddleWater puddle)
                        {
                            puddle.IgnitionAmount += 9;
                            if (puddle.IgnitionAmount >= Globals.IGNITION_MAX)
                            {
                                puddle.Destroy();
                            }
                            else if (!hasMadeFloater)
                            {
                                var startOffset = new Vector2(Game.Random.Float(-15f, 15f), Game.Random.Float(-15f, 15f));
                                var endOffset = startOffset + new Vector2(0f, Game.Random.Float(-15f, 0f));
                                var time = Utils.Map(puddle.IgnitionAmount, 0, Globals.IGNITION_MAX, 0.25f, 1.25f, EasingType.Linear) * Game.Random.Float(0.9f, 1.2f);
                                var scale = Utils.Map(puddle.IgnitionAmount, 0, Globals.IGNITION_MAX, 0.4f, 0.75f, EasingType.QuadIn);
                                var opacity = Utils.Map(puddle.IgnitionAmount, 0, Globals.IGNITION_MAX, 0.33f, 1f, EasingType.QuadIn);
                                var shakeAmount = Utils.Map(puddle.IgnitionAmount, 0, Globals.IGNITION_MAX, 0.1f, 1f, EasingType.Linear);

                                puddle.AddFloater("☁️", time, startOffset, endOffset, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.025f, scale: scale, opacity: opacity, shakeAmount: shakeAmount);
                                hasMadeFloater = true;
                            }
                        }
                    }
                }
            }
        }
    }

    public override void OnRemove()
    {
        Thing.RemoveTrait(Trait);

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);

        Thing.RemoveFloater(Globals.Icon(IconType.Burning));
        Thing.ContainingGridManager.ThingFloaterCounter++;
    }

    public override void OnThingDied()
    {
        Remove();
    }
}