using Sandbox;
using System;

namespace Roguemoji;
public partial class ProjectileArrow : Thing
{
	public ProjectileArrow()
	{
		DisplayIcon = "🔰";
        DisplayName = "Arrow";
        Description = "";
        Tooltip = "";
        IconDepth = 0;
        ShouldLogBehaviour = true;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
        }
    }

    public override void OnBumpedIntoThing(Thing thing)
    {
        base.OnBumpedIntoThing(thing);
        Destroy();
    }
}
