using Sandbox;
using System;

namespace Roguemoji;

public class VfxFly : ThingComponent
{
    public IntVector StartingGridPos { get; set; }
    public float Lifetime { get; set; }
    public float HeightY { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientComponent = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        float progress = Utils.Map(TimeSinceStart, 0f, Lifetime, 1f, 0f, EasingType.ExpoOut);
        IntVector deltaGrid = StartingGridPos - Thing.GridPos;
        float yOffset = Utils.MapReturn(progress, 0f, 1f, 0f, -HeightY, EasingType.QuadInOut);
        Thing.SetOffset(deltaGrid * 40f * progress + new Vector2(0f, yOffset));

        if(TimeSinceStart > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetOffset(Vector2.Zero);
    }
}