using Sandbox;
using System;

namespace Interfacer;

public class VfxSlide : CellVfx
{
    public Direction Direction { get; set; }
    public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Update(float dt)
    {
        var dir = GridManager.GetVectorForDirection(Direction);
        GridCell.VfxOffset = dir * Utils.Map(TimeSinceStart, 0f, Lifetime, -Distance, 0f, EasingType.ExpoOut);
        GridPanel.RefreshGridPos(GridPos);

        if (TimeSinceStart > Lifetime)
            Remove();
    }
}