using Sandbox;
using System;

namespace Roguemoji;
public partial class PuddleOil : Thing
{
    private float _elapsedTime;
    private int _iconState;

	public PuddleOil()
	{
		DisplayIcon = "⚫️";
        DisplayName = "Puddle of Oil";
        Description = "The ground is covered with flammable oil";
        Tooltip = "A puddle of oil";
        IconDepth = (int)IconDepthLevel.Normal;
        ShouldUpdate = true;
        Flags = ThingFlags.Selectable | ThingFlags.Puddle;
        Flammability = 45;
        PathfindMovementCost = 1.5f;
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
