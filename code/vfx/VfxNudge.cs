using Sandbox;
using System;

namespace Interfacer;

public class VfxNudge : CellVfx
{
    public Direction Direction { get; set; }
    public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Update(float dt)
    {
        var dir = GridManager.GetVectorForDirection(Direction);
        //GridCell.VfxOffset = dir * Utils.MapReturn(TimeSinceStart, 0f, Lifetime, 0f, Distance, EasingType.QuadOut);
        //GridPanel.RefreshGridPos(GridPos);

        if (TimeSinceStart > Lifetime)
            Remove();
    }


    public override void OnRemove()
    {
        
    }
}