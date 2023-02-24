using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public class COrganizeDebug : ThingComponent
{
    public float Lifetime { get; set; }
    public float Timer { get; set; }
    private float _delay = 0f;
    public int IconId { get; set; }
    private List<Thing> _orderedItems;
    private int _currIndex;

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        if (thing.GetComponent<CActing>(out var component))
            ((CActing)component).PreventAction();

        var player = thing.Brain as RoguemojiPlayer;
        if (player == null)
            return;

        if (thing is Smiley smiley && smiley.GetComponent<CIconPriority>(out var component2))
            IconId = ((CIconPriority)component2).AddIconPriority("🧐", (int)PlayerIconPriority.Organize);

        _orderedItems = player.InventoryGridManager.GetAllThings().Where(x => !IsInHotbar(x)).OrderBy(x => x.GetType().Name).ToList();
        _currIndex = 0;
    }
    
    bool IsInHotbar(Thing thing)
    {
        return thing.GridPos.y == 0 && thing.ContainingGridManager.GetIndex(thing.GridPos) < 10;
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

        while (_currIndex < _orderedItems.Count)
        {
            var thing = _orderedItems.ElementAt(_currIndex);
            if (thing != null && thing.ContainingGridType == GridType.Inventory && thing.ContainingGridManager.OwningPlayer == player)
            {
                int startingIndex = Math.Min(player.InventoryGridManager.GridWidth, 10);
                //int startingIndex = player.InventoryGridManager.GetIndex(new IntVector(0, 1));
                player.SwapGridThingPos(thing, GridType.Inventory, player.InventoryGridManager.GetGridPos(startingIndex + _currIndex), shouldAnimate: false);
            }

            _currIndex++;
        }

        Remove();
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
            iconPriority.AddIconPriority("🧐", (int)PlayerIconPriority.Organize, 0.33f);
        }
    }

    public override void OnThingDied()
    {
        Remove();
    }
}