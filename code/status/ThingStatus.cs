using Sandbox;
using System;

namespace Roguemoji;

public abstract class ThingStatus
{
    public Thing Thing { get; private set; }

    public bool ShouldUpdate { get; protected set; }

    public TimeSince TimeSinceStart { get; protected set; }
    public bool IsClientStatus { get; protected set; }

    public virtual void Init(Thing thing)
    {
        Thing = thing;
        ShouldUpdate = false;
        TimeSinceStart = 0f;
    }

    public virtual void Update(float dt)
    {
        if(IsClientStatus == Host.IsServer)
        {
            Log.Error(GetType().Name + " IsClientStatus: " + IsClientStatus + " Host.IsServer: " + Host.IsServer + "!");
        }
    }

    // status was added when already existing
    public virtual void ReInitialize()
    {
        TimeSinceStart = 0f;
    }

    public void Remove()
    {
        Thing.RemoveStatus(TypeLibrary.GetDescription(GetType()));
    }

    public virtual void OnRemove()
    {

    }
}