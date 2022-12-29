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
        IconDepth = 8;
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

    public override void OnRemoveComponent(TypeDescription type)
    {
        base.OnRemoveComponent(type);

        if(type == TypeLibrary.GetType(typeof(Projectile)))
            Destroy();
    }

    // todo: test when a solid thing moves onto your cell
}
