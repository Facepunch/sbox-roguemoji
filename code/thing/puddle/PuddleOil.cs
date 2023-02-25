using Sandbox;
using System;

namespace Roguemoji;
public partial class PuddleOil : Puddle
{
	public PuddleOil()
	{
		DisplayIcon = "⚫️";
        DisplayName = "Puddle of Oil";
        Description = "Flammable and slippery";
        Tooltip = "A puddle of oil";
        Flammability = 65;
        PathfindMovementCost = 0.1f;
        LiquidType = PotionType.Oil;
    }

    // todo: make things moving on it slide
    // todo: make splashing noise when you move onto it
    // todo: make visible when walking onto this while invisible

    public override void Update(float dt)
    {
        base.Update(dt);

        _elapsedTime += dt;

        if(_iconState == 0 && _elapsedTime > 0.3f)
        {
            _iconState++;
            DisplayIcon = "⬛️";
            IconDepth = (int)IconDepthLevel.Puddle;
            ShouldUpdate = false;
        }
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);

        if(!thing.LastGridPos.Equals(GridPos))
        {
            var projectile = thing.AddComponent<CProjectile>();
            projectile.Direction = GridManager.GetDirectionForIntVector(GridPos - thing.LastGridPos);
            projectile.MoveDelay = 0.15f;
            projectile.TotalDistance = 1;
            projectile.Thrower = null;
        }
    }
}
