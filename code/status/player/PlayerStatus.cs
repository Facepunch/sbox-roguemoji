using Sandbox;
using System;

namespace Roguemoji;

public abstract class PlayerStatus
{
    public RoguemojiPlayer Player { get; private set; }

    public bool ShouldUpdate { get; protected set; }

    public TimeSince TimeSinceStart { get; protected set; }
    public bool IsClientStatus { get; protected set; }

    public virtual void Init(RoguemojiPlayer player)
    {
        Player = player;
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
        Player.RemovePlayerStatus(TypeLibrary.GetDescription(GetType()));
    }

    public virtual void OnRemove()
    {

    }
}