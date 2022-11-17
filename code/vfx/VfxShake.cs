using Sandbox;
using System;

namespace Interfacer;

public class VfxShake : CellVfx
{
    public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Update(float dt)
    {
        var dir = Utils.DegreesToVector(Rand.Float(0f, 360f));
        //GridCell.VfxOffset = dir * Utils.Map(TimeSinceStart, 0f, Lifetime, Distance, 0f, EasingType.QuadOut);
        //GridPanel.RefreshGridPos(GridPos);

        if (TimeSinceStart > Lifetime)
            Remove();
    }
}