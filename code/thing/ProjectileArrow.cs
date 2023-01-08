using Sandbox;
using System;

namespace Roguemoji;
public partial class ProjectileArrow : Thing
{
    public Direction Direction { get; set; }

	public ProjectileArrow()
	{
		DisplayIcon = "🔰";
        DisplayName = "Arrow";
        Description = "";
        Tooltip = "";
        IconDepth = 8;

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

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);
        DamageOther(thing, Direction);
        Destroy();
    }

    public override void OnRemoveComponent(TypeDescription type)
    {
        base.OnRemoveComponent(type);

        if(type == TypeLibrary.GetType(typeof(CompProjectile)))
            Destroy();
    }

    // todo: test when a solid thing moves onto your cell
}
