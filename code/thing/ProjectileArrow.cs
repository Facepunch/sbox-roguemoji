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
        IconDepth = (int)IconDepthLevel.Projectile;

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
        Destroy();
    }

    public override void OnRemoveComponent(TypeDescription type)
    {
        base.OnRemoveComponent(type);

        if(type == TypeLibrary.GetType(typeof(CProjectile)))
            Destroy();
    }
}
