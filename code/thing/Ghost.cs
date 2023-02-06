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
        else
        {
            WieldedThingOffset = new Vector2(20f, 17f);
            WieldedThingFontSize = 18;
            InfoWieldedThingOffset = new Vector2(38f, 38f);
            InfoWieldedThingFontSize = 32;
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
        Flags = ThingFlags.Selectable | ThingFlags.CanWieldThings | ThingFlags.DoesntBumpThings;
        //ActionDelay = TimeSinceAction = 0.5f;
        //IsActionReady = true;
        Faction = FactionType.Ghost;
        IsInTransit = false;
        FloaterNum = 0;

        ClearStats();
        InitStat(StatType.Speed, 13);
        InitStat(StatType.Sight, 7, min: 0); // setting this will RefreshVisibility for the player
        InitStat(StatType.Hearing, 3);
        InitStat(StatType.SightBlockAmount, 1);
        InitStat(StatType.Invisible, 1);
        InitStat(StatType.Perception, 2);
        //InitStat(StatType.Smell, 1);

        ClearTraits();
    }

    public override bool TryMove(Direction direction, out bool switchedLevel, bool shouldAnimate = true, bool shouldQueueAction = false, bool dontRequireAction = false)
	{
        switchedLevel = false;

        if (IsInTransit)
            return false;

        var success = base.TryMove(direction, out switchedLevel, shouldAnimate, shouldQueueAction: false, dontRequireAction);

        if(!dontRequireAction)
            Acting.PerformedAction();

		return success;
	}

}
