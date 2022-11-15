using Sandbox;
using System;

namespace Interfacer;

public abstract class ThingStatus
{
    public Thing Thing { get; private set; }

    public bool ShouldUpdate { get; protected set; }

    public TimeSince TimeSinceStart { get; protected set; }

    public virtual void Init(Thing thing)
    {
        Thing = thing;
        ShouldUpdate = false;
        TimeSinceStart = 0f;
    }

    public virtual void Update(float dt)
    {
        
    }

    // status was added when already existing
    public virtual void ReInitialize()
    {

    }

    public void Remove()
    {
        Thing.RemoveStatus(TypeLibrary.GetDescription(GetType()));
    }

    public virtual void OnRemove()
    {

    }
}