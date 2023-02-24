using Sandbox;
using System;

namespace Roguemoji;

//public enum LiquidType { Water, Lava, Blood, Mud, Oil, Piss, ToxicSludge, Snow, Purple }

public partial class Puddle : Thing
{
    protected float _elapsedTime;
    protected int _iconState;

    public PotionType LiquidType { get; protected set; }

	public Puddle()
	{
        IconDepth = (int)IconDepthLevel.Normal;
        ShouldUpdate = true;
        Flags = ThingFlags.Selectable | ThingFlags.Puddle | ThingFlags.CantBePushed;
    }
}
