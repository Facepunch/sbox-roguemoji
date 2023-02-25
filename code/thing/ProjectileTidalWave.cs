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

    public override void OnMovedOntoThing(Thing thing)
    {
        base.OnMovedOntoThing(thing);

        if(!LastGridPos.Equals(GridPos) && !thing.HasFlag(ThingFlags.CantBePushed))
        {
            if(!thing.TryMove(GridManager.GetDirectionForIntVector(GridPos - LastGridPos), out bool switchedLevel, out bool actionWasntReady, dontRequireAction: true))
                thing.Hurt(SlamDamage);
        }
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);

        if (!LastGridPos.Equals(GridPos) && !thing.HasFlag(ThingFlags.CantBePushed))
        {
            if (!thing.TryMove(GridManager.GetDirectionForIntVector(GridPos - LastGridPos), out bool switchedLevel, out bool actionWasntReady, dontRequireAction: true))
                thing.Hurt(SlamDamage);
        }
    }

    public override void OnChangedGridPos()
    {
        base.OnChangedGridPos();

        if (!LastGridPos.Equals(GridPos))
        {
            if (!ContainingGridManager.DoesGridPosContainThingType<PuddleWater>(LastGridPos))
            {
                ContainingGridManager.RemovePuddles(LastGridPos, fadeOut: true);
                ContainingGridManager.SpawnThing<PuddleWater>(LastGridPos);
            }
        }
    }

    public void PushStartPosThings(IntVector gridDir)
    {
        var things = ContainingGridManager.GetThingsAt(GridPos).WithNone(ThingFlags.CantBePushed).Where(x => x != this).ToList();
        foreach(var thing in things)
        {
            if (!thing.TryMove(GridManager.GetDirectionForIntVector(gridDir), out bool switchedLevel, out bool actionWasntReady, dontRequireAction: true))
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
