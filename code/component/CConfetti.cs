using Sandbox;
using System;
using System.Collections.Generic;
namespace Roguemoji;

public class CConfetti : ThingComponent
{
    public float Lifetime { get; set; }
    public float DropTimer { get; set; }
    private float _dropDelay = 0.05f;
    private int _dropRange = 3;
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        if (thing.GetComponent<CActing>(out var component))
            ((CActing)component).PreventAction();

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component2))
            IconId = ((CIconPriority)component2).AddIconPriority("🥳", (int)PlayerIconPriority.Confetti);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var player = Thing.Brain as RoguemojiPlayer;
        if(player == null)
        {
            Remove();
            return;
        }

        DropTimer += dt;
        if(DropTimer >= _dropDelay)
        {
            if(player.InventoryGridManager.Things.Count > 0)
            {
                if (Thing.ContainingGridManager.GetRandomEmptyGridPosWithinRange(Thing.GridPos, out var emptyGridPos, _dropRange, allowNonSolid: true))
                    DropItem(player, player.InventoryGridManager.Things[0], emptyGridPos);
            }
            else if (player.EquipmentGridManager.Things.Count > 0)
            {
                if (Thing.ContainingGridManager.GetRandomEmptyGridPosWithinRange(Thing.GridPos, out var emptyGridPos, _dropRange, allowNonSolid: true))
                    DropItem(player, player.EquipmentGridManager.Things[0], emptyGridPos);
            }
            else
            {
                Remove();
                return;
            }

            DropTimer -= Game.Random.Float(0f, _dropDelay);
        }
    }

    void DropItem(RoguemojiPlayer player, Thing item, IntVector gridPos)
    {
        player.MoveThingTo(item, GridType.Arena, gridPos, dontRequireAction: true);

        var time = Game.Random.Float(0.3f, 0.4f);
        item.VfxFly(player.ControlledThing.GridPos, lifetime: time, heightY: Game.Random.Float(20f, 55f), progressEasingType: EasingType.Linear, heightEasingType: EasingType.QuadInOut);

        item.CanBeSeenByPlayerClient(player.ControlledThing.GridPos);

        var tempIconDepth = item.AddComponent<CTempIconDepth>();
        tempIconDepth.Lifetime = time;
        tempIconDepth.SetTempIconDepth((int)IconDepthLevel.Projectile);
    }

    public override void OnRemove()
    {
        if (Thing.Brain is RoguemojiPlayer player)
            player.ClearQueuedAction();
            
        if (Thing.GetComponent<CActing>(out var component))
            ((CActing)component).AllowAction();

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component2))
        {
            var iconPriority = ((CIconPriority)component2);
            iconPriority.RemoveIconPriority(IconId);
            iconPriority.AddIconPriority("🥳", (int)PlayerIconPriority.Confetti, 0.5f);
        }
    }

    public override void OnThingDied()
    {
        Remove();
    }
}