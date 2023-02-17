using Sandbox;
using System;

namespace Roguemoji;
public partial class PuddleLava : Puddle
{
    public float FloaterCountdown { get; set; }
    public float FloaterDelayMin { get; set; }
    public float FloaterDelayMax { get; set; }

	public PuddleLava()
	{
		DisplayIcon = "🟠";
        DisplayName = "Puddle of Lava";
        Description = "The ground is covered with burning lava";
        Tooltip = "A puddle of lava";
        Flammability = 0;
        PathfindMovementCost = 11f;
        LiquidType = PotionType.Lava;

        FloaterDelayMin = 0.85f;
        FloaterDelayMax = 1.75f;
        FloaterCountdown = Game.Random.Float(FloaterDelayMin, FloaterDelayMax);
    }

    // todo: make splashing noise when you move onto it
    // todo: make visible when walking onto this while invisible

    public override void Update(float dt)
    {
        base.Update(dt);

        _elapsedTime += dt;

        if(_iconState == 0 && _elapsedTime > 0.3f)
        {
            _iconState++;
            DisplayIcon = "🟧";
            IconDepth = (int)IconDepthLevel.Puddle;
        }

        FloaterCountdown -= dt;
        if (FloaterCountdown < 0f)
        {
            var startOffset = new Vector2(Game.Random.Float(-15f, 15f), Game.Random.Float(-15f, 15f));
            var endOffset = startOffset + new Vector2(0f, Game.Random.Float(-25f, -5f));
            var time = Game.Random.Float(0.25f, 0.85f);
            var scale = Game.Random.Float(0.45f, 0.65f);
            var opacity = Game.Random.Float(0.25f, 0.66f);
            var shakeAmount = Game.Random.Float(0.1f, 0.4f);
            var fadeInTime = Game.Random.Float(0.015f, 0.2f);

            ContainingGridManager.AddFloater(Globals.Icon(IconType.Burning), GridPos, time, startOffset, endOffset, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: fadeInTime, scale: scale, opacity: opacity, shakeAmount: shakeAmount);

            FloaterCountdown = Game.Random.Float(FloaterDelayMin, FloaterDelayMax);
        }
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);
        IgniteThing(thing);
    }

    public override void OnMovedOntoThing(Thing thing)
    {
        base.OnMovedOntoThing(thing);
        IgniteThing(thing);
    }

    public override void OnBumpedIntoThing(Thing thing, Direction direction)
    {
        base.OnBumpedIntoThing(thing, direction);
        IgniteThing(thing);
    }

    void IgniteThing(Thing thing)
    {
        if (!thing.ContainingGridManager.ShouldCellPutOutFire(thing.GridPos) && thing.Flammability > 0)
        {
            var burning = thing.AddComponent<CBurning>();
            burning.Lifetime = 30f;
        }
    }
}
