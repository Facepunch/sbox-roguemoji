using Sandbox;
using System;

namespace Roguemoji;

public class VfxFly : ThingComponent
{
    public IntVector StartingGridPos { get; set; }
    public float Lifetime { get; set; }
    public float HeightY { get; set; }
    public EasingType ProgressEasingType { get; set; }
    public EasingType HeightEasingType { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientComponent = true;
        ProgressEasingType = EasingType.ExpoOut;
        HeightEasingType = EasingType.QuadInOut;

        SetOffset(progress: 0f);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        float progress = Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f, ProgressEasingType);
        SetOffset(progress);

        if(TimeElapsed > Lifetime)
            Remove();
    }

    void SetOffset(float progress)
    {
        IntVector deltaGrid = StartingGridPos - Thing.GridPos;
        float yOffset = Utils.MapReturn(progress, 0f, 1f, 0f, -HeightY, HeightEasingType);
        Thing.SetOffset(deltaGrid * 40f * (1f - progress) + new Vector2(0f, yOffset));
    }

    public override void OnRemove()
    {
        Thing.SetOffset(Vector2.Zero);
    }
}