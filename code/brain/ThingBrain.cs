using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class ThingBrain : Entity
{
    [Net] public Thing ControlledThing { get; private set; }

    public virtual void ControlThing(Thing thing)
    {
        ControlledThing = thing;
        thing.Brain = this;
    }

    public virtual void Update(float dt)
    {

    }

    [Event.Tick.Client]
    public virtual void ClientTick()
    {

    }

    public virtual void HearSound(string name, IntVector soundPos, int loudness = 0, float volume = 1f, float pitch = 1f)
    {

    }

    public virtual void OnTakeDamageFrom(Thing thing) { }
    public virtual void OnHurt(int amount) { }
    public virtual void OnChangedGridPos() { }
    public virtual void OnMove(Direction direction) { }
    public virtual void OnChangedStat(StatType statType, int changeCurrent, int changeMin, int changeMax) { }
    public virtual void OnWieldThing(Thing thing) { }
    public virtual void OnActionRecharged() { }
    public virtual void OnFindTarget(Thing target) { }
    public virtual void OnLoseTarget() { }
    public virtual void OnDestroyed() { }
    public virtual void OnWieldedThingBumpedOther(Thing thing, Direction direction) { }
    public virtual void OnBumpedIntoThing(Thing thing, Direction direction) { }
    public virtual void OnBumpedIntoBy(Thing thing, Direction direction) { }
    public virtual void OnBumpedOutOfBounds(Direction direction) { }
    public virtual void OnUseThing(Thing thing) { }
}