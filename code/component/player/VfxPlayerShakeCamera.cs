using Sandbox;
using System;

namespace Roguemoji;

public class VfxPlayerShakeCamera : PlayerComponent
{
    public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Init(RoguemojiPlayer player)
    {
        base.Init(player);

        ShouldUpdate = true;
        IsClientComponent = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var dir = Utils.DegreesToVector(Game.Random.Float(0f, 360f));
        Player.SetCameraPixelOffset(dir * Utils.Map(TimeSinceStart, 0f, Lifetime, Distance, 0f, EasingType.QuadOut));

        if(TimeSinceStart > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Player.SetCameraPixelOffset(Vector2.Zero);
    }
}