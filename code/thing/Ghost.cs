using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Ghost : Thing
{
    public CActing Acting { get; private set; }

    // todo: make inflammable

    public Ghost()
	{
        ShouldUpdate = true;
		DisplayName = "Ghost";
		Tooltip = "";
        PathfindMovementCost = 0f;
        Flammability = 0;

        if (Game.IsServer)
        {
            SetStartingValues();
        }
	}

    public override void Spawn()
    {
        base.Spawn();

        Acting = AddComponent<CActing>();
    }

    //[Event.Tick.Client]
    //public virtual void ClientTick()
    //{
    //    base.ClientTick();

    //    DrawDebugText($"{GridPos}");
    //}

    void SetStartingValues()
    {
        DisplayIcon = "👻";
        IconDepth = (int)IconDepthLevel.Ghost;
        Flags = ThingFlags.Selectable | ThingFlags.CanWieldThings | ThingFlags.DoesntBumpThings | ThingFlags.CantBePushed;
        //ActionDelay = TimeSinceAction = 0.5f;
        //IsActionReady = true;
        Faction = FactionType.Ghost;
        IsInTransit = false;
        FloaterNum = 0;

        ClearStats();
        InitStat(StatType.Speed, 13);
        InitStat(StatType.SightDistance, 7, min: 0); // setting this will RefreshVisibility for the player
        InitStat(StatType.SightPenetration, 7);
        InitStat(StatType.Hearing, 3);
        InitStat(StatType.SightBlockAmount, 1);
        InitStat(StatType.Invisible, 1);
        InitStat(StatType.Perception, 2);
        //InitStat(StatType.Smell, 1);

        ClearTraits();
    }
}
