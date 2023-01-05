using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;

namespace Roguemoji;

public partial class Level : Entity
{
	[Net] public GridManager GridManager { get; private set; }

    [Net] public LevelId LevelId { get; private set; }

    [Net] public string LevelName { get; private set; }

    public void Init(LevelId levelId)
    {
        LevelId = levelId;

        GetLevelSize(levelId, out var width, out var height);

        GridManager = new();
        GridManager.Init(width, height);
        GridManager.GridType = GridType.Arena;
        GridManager.LevelId = LevelId;

        LevelName = GetLevelName(levelId);

        Transmit = TransmitType.Always;

        SpawnStartingThings(levelId);
    }

    public void Update(float dt)
    {
        GridManager.Update(dt);
    }

    public void Restart()
    {
        GridManager.Restart();
        SpawnStartingThings(LevelId);
    }

    void GetLevelSize(LevelId levelId, out int width, out int height)
    {
        switch (levelId)
        {
            case LevelId.Forest0: width = 40; height = 25; break;
            case LevelId.Forest1: width = 30; height = 22; break;
            case LevelId.Forest2: width = 30; height = 22; break;
            default: width = 0; height = 0;break;
        }
    }

    string GetLevelName(LevelId levelId)
    {
        switch (levelId)
        {
            case LevelId.Forest0: return "Forest 1";
            case LevelId.Forest1: return "Forest 2";
            case LevelId.Forest2: return "Cheese Fields";
            default: return "???";
        }
    }

    public static string GetLevelBgColor(LevelId levelId, bool odd)
    {
        switch (levelId)
        {
            case LevelId.Forest0: return odd ? "#082b0f" : "#07270e";
            //case LevelId.Forest0: return odd ? "#083211" : "#082e10";
            case LevelId.Forest1: return odd ? "#051609" : "#041408";
            case LevelId.Forest2: return odd ? "#051609" : "#041408";
            default: return "#000000";
        }
    }

    void SpawnStartingThings(LevelId levelId)
    {
        if(levelId == LevelId.Forest0)
        {
            for (int x = 0; x < GridManager.GridWidth; x++)
            {
                GridManager.SpawnThing<OilBarrel>(new IntVector(x, 0));
                GridManager.SpawnThing<OilBarrel>(new IntVector(x, GridManager.GridHeight - 1));
            }

            for (int y = 1; y < GridManager.GridHeight - 1; y++)
            {
                GridManager.SpawnThing<OilBarrel>(new IntVector(0, y));
                GridManager.SpawnThing<OilBarrel>(new IntVector(GridManager.GridWidth - 1, y));
            }

            //GridManager.SpawnThing<Golem>(new IntVector(10, 10));
            GridManager.SpawnThing<Leaf>(new IntVector(9, 10));
            GridManager.SpawnThing<Leaf>(new IntVector(21, 19));

            {
                if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                    GridManager.SpawnThing<Hole>(gridPos);
            }

            for (int i = 0; i < 50; i++)
            {
                if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                    GridManager.SpawnThing<TreeEvergreen>(gridPos);
            }

            for (int i = 0; i < 50; i++)
            {
                if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                    GridManager.SpawnThing<TreeDeciduous>(gridPos);
            }

            for (int i = 0; i < 2; i++)
            {
                if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                    GridManager.SpawnThing<Squirrel>(gridPos);
            }
        }
        else if(levelId == LevelId.Forest1)
        {
            for (int i = 0; i < 5; i++)
            {
                if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                    GridManager.SpawnThing<Squirrel>(gridPos);
            }

            {
                if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                    GridManager.SpawnThing<Hole>(gridPos);
            }

            {
                if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                    GridManager.SpawnThing<Door>(gridPos);
            }
        }
        else if (levelId == LevelId.Forest2)
        {
            for (int i = 0; i < 25; i++)
            {
                if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                    GridManager.SpawnThing<Cheese>(gridPos);
            }

            {
                if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                    GridManager.SpawnThing<Door>(gridPos);
            }
        }
    }
}
