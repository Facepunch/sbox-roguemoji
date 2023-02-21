using Sandbox;
using System;

namespace Roguemoji;
public partial class PuddleOil : Puddle
{
	public PuddleOil()
	{
		DisplayIcon = "⚫️";
        DisplayName = "Puddle of Oil";
        Description = "The ground is covered with flammable oil";
        Tooltip = "A puddle of oil";
        Flammability = 65;
        PathfindMovementCost = 1.5f;
        LiquidType = PotionType.Oil;
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
            DisplayIcon = "⬛️";
            IconDepth = (int)IconDepthLevel.Puddle;
            ShouldUpdate = false;
        }
    }
}
