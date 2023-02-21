using Sandbox;
using System;

namespace Roguemoji;

public class VfxPlayerFlashCamera : PlayerComponent
{
    public float Lifetime { get; set; }
    public Color Color { get; set; }

    public override void Init(RoguemojiPlayer player)
    {
        base.Init(player);

        ShouldUpdate = true;
        IsClientComponent = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        Player.CameraFade = Utils.MapReturn(TimeElapsed, 0f, Lifetime, 0f, 1f, EasingType.Linear);
        Player.CameraFadeColor = Color;

        if (TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Player.CameraFade = 0f;
    }
}