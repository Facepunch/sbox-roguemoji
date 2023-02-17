using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class Basketball : Thing
{
    public Basketball()
	{
		DisplayIcon = "🏀";
        DisplayName = "Basketball";
        Description = "Bounces back unless it's caught";
        Tooltip = "A basketball";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;
        Flammability = 15;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);

            AddTrait("", "🙌", $"When 🏀 hits, target catches if possible/Otherwise 🏀 disarms target and bounces back", offset: new Vector2(0f, -1f), tattooIcon: "🏀", tattooOffset: new Vector2(0f, -12f), tattooScale: 0.65f);
        }
    }
    
    public override void OnBumpedOutOfBounds(Direction direction)
    {
        if (!HasComponent<CProjectile>())
        {
            base.OnBumpedOutOfBounds(direction);
            return;
        }

        Thing thrower = null;
        float moveDelay = 0.1f;
        int distance = 5;

        if (GetComponent<CProjectile>(out var component))
        {
            var p = ((CProjectile)component);
            thrower = p.Thrower;
            moveDelay = p.MoveDelay;
            distance = p.TotalDistance;
        }

        base.OnBumpedOutOfBounds(direction);

        AddProjectile(GridManager.GetOppositeDirection(direction), moveDelay, distance, thrower);
    }

    public override void HitOther(Thing target, Direction direction)
    {
        if(!HasComponent<CProjectile>())
        {
            base.HitOther(target, direction);
            return;
        }

        Thing thrower = null;
        float moveDelay = 0.1f;
        int distance = 5;

        if (GetComponent<CProjectile>(out var component))
        {
            var p = ((CProjectile)component);
            thrower = p.Thrower;
            moveDelay = p.MoveDelay;
            distance = p.TotalDistance - 1;
        }

        if (target.HasFlag(ThingFlags.CanWieldThings))
        {
            if(target.WieldedThing == null)
            {
                if(target.Brain is RoguemojiPlayer player)
                {
                    if(!player.TryPickUp(this))
                    {
                        RemoveComponent<CProjectile>();
                        AddProjectile(GridManager.GetOppositeDirection(direction), moveDelay, distance, thrower);
                    }
                }
                else
                {
                    target.WieldAndRemoveFromGrid(this);
                    target.VfxShake(0.2f, 4f);
                }
            }
            else
            {
                target.TryDropThingNearby(target.WieldedThing);
                target.VfxShake(0.2f, 4f);

                RemoveComponent<CProjectile>();
                AddProjectile(GridManager.GetOppositeDirection(direction), moveDelay, distance, thrower);
            }
        }
        else
        {
            RemoveComponent<CProjectile>();
            base.HitOther(target, direction);
            AddProjectile(GridManager.GetOppositeDirection(direction), moveDelay, distance, thrower);
        }
    }

    void AddProjectile(Direction dir, float moveDelay, int distance, Thing thrower)
    {
        CProjectile projectile = AddComponent<CProjectile>();
        projectile.Direction = dir;
        projectile.MoveDelay = moveDelay;
        projectile.TotalDistance = distance;
        projectile.Thrower = thrower;
    }
}
