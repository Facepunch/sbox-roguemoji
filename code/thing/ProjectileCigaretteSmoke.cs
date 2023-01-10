using Sandbox;
using System;

namespace Roguemoji;
public partial class ProjectileCigaretteSmoke : Thing
{
    public Direction Direction { get; set; }

	public ProjectileCigaretteSmoke()
	{
		DisplayIcon = "💨";
        DisplayName = "Cigarette Smoke";
        Description = "";
        Tooltip = "";
        IconDepth = 8;
        Flags = ThingFlags.DoesntBumpThings;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
        }
    }

    public override void OnMovedOntoThing(Thing thing)
    {
        base.OnMovedOntoThing(thing);

        thing.TakeDamage(this);
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);

        thing.TakeDamage(this);
    }

    public override void OnRemoveComponent(TypeDescription type)
    {
        base.OnRemoveComponent(type);

        if(type == TypeLibrary.GetType(typeof(CProjectile)))
            Destroy();
    }
}
