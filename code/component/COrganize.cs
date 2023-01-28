﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public class COrganize : ThingComponent
{
    public float Lifetime { get; set; }
    public float Timer { get; set; }
    private float _delay = 0.03f;
    public int IconId { get; set; }
    private List<Thing> _orderedItems;
    private int _currIndex;

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        if (thing.GetComponent<CActing>(out var component))
            ((CActing)component).PreventAction();

        var player = thing as RoguemojiPlayer;
        if (player == null)
            return;

        if (player.GetComponent<CIconPriority>(out var component2))
            IconId = ((CIconPriority)component2).AddIconPriority("🧐", (int)PlayerIconPriority.Organize);

        _orderedItems = player.InventoryGridManager.GetAllThings().Where(x => x.GridPos.y > 0).OrderBy(x => x.GetType().Name).ToList();
        _currIndex = 0;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var player = Thing as RoguemojiPlayer;
        if(player == null)
        {
            Remove();
            return;
        }

        Timer += dt;
        if(Timer >= _delay)
        {
            if (_currIndex < _orderedItems.Count)
            {
                var thing = _orderedItems.ElementAt(_currIndex);
                if(thing != null && thing.ContainingGridType == GridType.Inventory && thing.ContainingGridManager.OwningPlayer == player) 
                {
                    RoguemojiGame.Instance.AddFloaterInventory(player, "📥️", thing.GridPos, 0.5f, new Vector2(0f, 0f), new Vector2(0, -10f), height: 0f, text: "", EasingType.QuadOut, fadeInTime: 0.01f, scale: 1f, opacity: 1f, parent: thing);

                    int startingIndex = player.InventoryGridManager.GetIndex(new IntVector(0, 1));
                    player.SwapGridThingPos(thing, GridType.Inventory, player.InventoryGridManager.GetGridPos(startingIndex + _currIndex));
                }

                _currIndex++;
            }
            else
            {
                Remove();
                return;
            }

            Timer -= _delay;
        }
    }

    public override void OnRemove()
    {
        if (Thing is RoguemojiPlayer player)
            player.ClearQueuedAction();
            
        if (Thing.GetComponent<CActing>(out var component))
            ((CActing)component).AllowAction();

        if (Thing is RoguemojiPlayer && Thing.GetComponent<CIconPriority>(out var component2))
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