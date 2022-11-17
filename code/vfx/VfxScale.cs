using Sandbox;
using System;

namespace Interfacer;

public class VfxScale : CellVfx
{
    public float Lifetime { get; set; }
    public float StartScale { get; set; }
    public float EndScale { get; set; }

    public override void Update(float dt)
    {
        //GridCell.VfxScale = Utils.Map(TimeSinceStart, 0f, Lifetime, StartScale, EndScale, EasingType.Linear);
        //GridPanel.RefreshGridPos(GridPos);

        if (TimeSinceStart > Lifetime)
            Remove();
    }
}