using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class ThingBrain : Entity
{
    [Net] public Thing ControlledThing { get; set; }

    public virtual void Update(float dt)
    {

    }

    [Event.Tick.Client]
    public virtual void ClientTick()
    {

    }

    public virtual void OnChangedGridPos() { }
    public virtual void OnChangedStat(StatType statType, int changeCurrent, int changeMin, int changeMax) { }
    public virtual void OnWieldThing(Thing thing) { }
    public virtual void OnActionRecharged() { }
}