using Sandbox;
using System;

namespace Roguemoji;
public partial class PuddleMud : Thing
{
    private float _elapsedTime;
    private int _iconState;

	public PuddleMud()
	{
		DisplayIcon = "🟤";
        DisplayName = "Puddle of Mud";
        Description = "The ground is covered with sticky mud";
        Tooltip = "A puddle of mud";
        IconDepth = (int)IconDepthLevel.Normal;
        ShouldUpdate = true;
        Flags = ThingFlags.Selectable | ThingFlags.Puddle;
        Flammability = 0;
        PathfindMovementCost = 4f;
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
