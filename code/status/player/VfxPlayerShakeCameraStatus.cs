using Sandbox;
using System;

namespace Roguemoji;

public class VfxPlayerShakeCameraStatus : PlayerStatus
{
    public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Init(RoguemojiPlayer player)
    {
        base.Init(player);

        ShouldUpdate = true;
        IsClientStatus = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var dir = Utils.DegreesToVector(Rand.Float(0f, 360f));
        Player.SetCameraPixelOffset(dir * Utils.Map(TimeSinceStart, 0f, Lifetime, Distance, 0f, EasingType.QuadOut));

        if(TimeSinceStart > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Player.SetCameraPixelOffset(Vector2.Zero);
    }
}