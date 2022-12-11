using Sandbox;
using System;

namespace Roguemoji;

public abstract class PlayerComponent
{
    public RoguemojiPlayer Player { get; private set; }

    public bool ShouldUpdate { get; protected set; }

    public TimeSince TimeSinceStart { get; protected set; }
    public bool IsClientComponent { get; protected set; }

    public virtual void Init(RoguemojiPlayer player)
    {
        Player = player;
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
        Player.RemovePlayerComponent(TypeLibrary.GetType(GetType()));
    }

    public virtual void OnRemove()
    {

    }
}