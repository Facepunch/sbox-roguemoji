using Sandbox;
using System;

namespace Roguemoji;
public partial class PuddleMud : Puddle
{
	public PuddleMud()
	{
		DisplayIcon = "🟤";
        DisplayName = "Puddle of Mud";
        Description = "The ground is covered with sticky mud";
        Tooltip = "A puddle of mud";
        Flammability = 0;
        PathfindMovementCost = 4f;
        LiquidType = PotionType.Mud;
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
            DisplayIcon = "🟫";
            IconDepth = (int)IconDepthLevel.Puddle;
            ShouldUpdate = false;
        }
    }
}
