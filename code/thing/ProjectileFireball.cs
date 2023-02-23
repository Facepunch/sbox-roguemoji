using Sandbox;
using System;
using System.Linq;

namespace Roguemoji;
public partial class ProjectileFireball : Thing
{
    public int ExplosionDamage { get; set; }

	public ProjectileFireball()
	{
		DisplayIcon = Globals.Icon(IconType.Fire);
        DisplayName = "Fireball";
        Description = "";
        Tooltip = "";
        IconDepth = (int)IconDepthLevel.Projectile;
        Flammability = 0;
    }

    public override void OnBumpedIntoThing(Thing thing, Direction direction)
    {
        base.OnBumpedIntoThing(thing, direction);
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

        if (type == TypeLibrary.GetType(typeof(CProjectile)))
            Explode(GridPos);
    }

    public override void HitOther(Thing target, Direction direction)
    {
        base.HitOther(target, direction);
        Explode(target.GridPos);
    }

    void Explode(IntVector explodeGridPos)
    {
        PlaySfx("fireball_explode", loudness: 5);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var gridPos = explodeGridPos + new IntVector(x, y);
                if (ContainingGridManager.IsGridPosInBounds(gridPos) && !ContainingGridManager.ShouldCellPutOutFire(gridPos))
                {
                    ContainingGridManager.AddFloater(Globals.Icon(IconType.Fire), gridPos, Game.Random.Float(0.7f, 0.9f), new Vector2(0f, 0f), new Vector2(0f, Game.Random.Float(-10f, -15f)), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true,
                        EasingType.QuadOut, fadeInTime: Game.Random.Float(0.01f, 0.05f), scale: Game.Random.Float(0.75f, 0.9f), opacity: 0.4f);

                    var things = ContainingGridManager.GetThingsAt(gridPos).Where(x => x != this && x.Flammability > 0).ToList();
                    for (int i = things.Count - 1; i >= 0; i--)
                    {
                        var thing = things[i];
                        thing.AddComponent<CBurning>();

                        if (thing.HasStat(StatType.Health))
                            thing.Hurt(ExplosionDamage);
                    }
                }
            }
        }

        Destroy();
    }
}
