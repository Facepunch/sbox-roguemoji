using Sandbox;
using System;

namespace Roguemoji;
public partial class PuddleBlood : Thing
{
    private float _elapsedTime;
    private int _iconState;

	public PuddleBlood()
	{
		DisplayIcon = "🩸";
        DisplayName = "Puddle of Blood";
        Description = "The ground is covered in a layer of blood";
        Tooltip = "A puddle of blood";
        IconDepth = (int)IconDepthLevel.Normal;
        ShouldUpdate = true;
        Flags = ThingFlags.Selectable;
        Flammability = 0;
    }

    // todo: make splashing noise when you move onto it
    // todo: make visible when walking onto this while invisible

    public override void Update(float dt)
    {
        base.Update(dt);

        _elapsedTime += dt;

        if(_iconState == 0 && _elapsedTime > 0.25f)
        {
            _iconState++;
            DisplayIcon = "🔴";
            IconDepth = (int)IconDepthLevel.Puddle;
        }
        else if(_iconState == 1 && _elapsedTime > 0.4f)
        {
            _iconState++;
            DisplayIcon = "🟥";
            ShouldUpdate = false;
        }
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);

        PuddleWater.MakeWet(thing);
    }

    public override void OnMovedOntoThing(Thing thing)
    {
        base.OnMovedOntoThing(thing);

        PuddleWater.MakeWet(thing);
    }
}
