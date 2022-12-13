using Sandbox;
using System;

namespace Roguemoji;

public abstract class ThingComponent
{
    public Thing Thing { get; private set; }

    public bool ShouldUpdate { get; protected set; }

    public TimeSince TimeSinceStart { get; protected set; }
    public bool IsClientComponent { get; protected set; }

    public virtual void Init(Thing thing)
    {
        Thing = thing;
        ShouldUpdate = false;
        TimeSinceStart = 0f;
    }

    public virtual void Update(float dt)
    {
        if(IsClientComponent == Game.IsServer)
        {
            Log.Error(GetType().Name + " IsClientComponent: " + IsClientComponent + " IsServer: " + Game.IsServer + "!");
        }
    }

    // component was added when already existing
    public virtual void ReInitialize()
    {
        TimeSinceStart = 0f;
    }

    public void Remove()
    {
        Thing.RemoveComponent(TypeLibrary.GetType(GetType()));
    }

    public virtual void OnRemove()
    {

    }
}