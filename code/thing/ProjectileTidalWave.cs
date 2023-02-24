using Sandbox;
using System;
using System.Linq;

namespace Roguemoji;
public partial class ProjectileTidalWave : Thing
{
    public int SlamDamage { get; set; }

	public ProjectileTidalWave()
	{
		DisplayIcon = Globals.Icon(IconType.Wave);
        DisplayName = "Wave";
        Description = "";
        Tooltip = "";
        IconDepth = (int)IconDepthLevel.Projectile;
        Flags = ThingFlags.DoesntBumpThings;
        Flammability = 0;
    }

    public override void OnMovedOntoThing(Thing thing, IntVector fromGridPos)
    {
        base.OnMovedOntoThing(thing, fromGridPos);

        if(!LastGridPos.Equals(GridPos) && !thing.HasFlag(ThingFlags.CantBePushed))
        {
            if(!thing.TryMove(GridManager.GetDirectionForIntVector(GridPos - LastGridPos), out bool switchedLevel))
                thing.Hurt(SlamDamage);
        }
    }

    public override void OnMovedOntoBy(Thing thing, IntVector fromGridPos)
    {
        base.OnMovedOntoBy(thing, fromGridPos);

        if (!LastGridPos.Equals(GridPos) && !thing.HasFlag(ThingFlags.CantBePushed))
        {
            if (!thing.TryMove(GridManager.GetDirectionForIntVector(GridPos - LastGridPos), out bool switchedLevel))
                thing.Hurt(SlamDamage);
        }
    }

    public override void OnChangedGridPos(IntVector fromGridPos)
    {
        base.OnChangedGridPos(fromGridPos);

        if (!fromGridPos.Equals(GridPos))
        {
            if (!ContainingGridManager.DoesGridPosContainThingType<PuddleWater>(fromGridPos))
            {
                ContainingGridManager.RemovePuddles(fromGridPos, fadeOut: true);
                ContainingGridManager.SpawnThing<PuddleWater>(fromGridPos);
            }
        }
    }

    public void PushStartPosThings(IntVector gridDir)
    {
        var things = ContainingGridManager.GetThingsAt(GridPos).WithNone(ThingFlags.CantBePushed).Where(x => x != this).ToList();
        foreach(var thing in things)
        {
            if (!thing.TryMove(GridManager.GetDirectionForIntVector(gridDir), out bool switchedLevel))
                thing.Hurt(SlamDamage);
        }
    }

    public override void OnRemoveComponent(TypeDescription type)
    {
        base.OnRemoveComponent(type);

        if (type == TypeLibrary.GetType(typeof(CProjectile)))
            Destroy();
    }
}
