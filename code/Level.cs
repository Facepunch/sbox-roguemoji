using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Sandbox;

namespace Roguemoji;

public partial class Level : Entity
{
	[Net] public GridManager GridManager { get; private set; }

    [Net] public LevelId LevelId { get; private set; }

    [Net] public string LevelName { get; private set; }

    public LevelData LevelData { get; private set; }

    public SurfaceType SurfaceType { get; private set; }

    [Net] public string BgColorEven { get; private set; }
    [Net] public string BgColorOdd { get; private set; }
    public string WalkSound { get; private set; }

    public void Init(LevelId levelId)
    {
        LevelId = levelId;

        LevelData = FileSystem.Mounted.ReadJson<LevelData>($"levels/{levelId}.json");

        if(LevelData == null)
        {
            Log.Error($"Level {levelId} not loaded!");
        }

        GridManager = new();
        GridManager.Init(LevelData.Width, LevelData.Height);
        GridManager.GridType = GridType.Arena;
        GridManager.LevelId = LevelId;

        LevelName = LevelData.Name;
        BgColorEven = LevelData.BgColorEven;
        BgColorOdd = LevelData.BgColorOdd;
        WalkSound = LevelData.WalkSound;

        Transmit = TransmitType.Always;

        SpawnStartingThings();
    }

    public void Update(float dt)
    {
        GridManager.Update(dt);
    }

    public void UpdateClient(float dt)
    {
        GridManager.UpdateClient(dt);
    }

    public void Restart()
    {
        GridManager.Restart();
        SpawnStartingThings();
    }

    void SpawnStartingThings()
    {
        if (LevelData.Things != null)
        {
            foreach (var pair in LevelData.Things)
            {
                var type = TypeLibrary.GetType<Thing>(pair.Key);

                foreach (var gridPos in pair.Value)
                    GridManager.SpawnThing(type, gridPos);
            }
        }

        if (LevelData.RandomThings != null)
        {
            foreach (var pair in LevelData.RandomThings)
            {
                var type = TypeLibrary.GetType<Thing>(pair.Key);

                for (int i = 0; i < pair.Value; i++)
                {
                    if (GridManager.GetRandomEmptyGridPos(out var gridPos))
                        GridManager.SpawnThing(type, gridPos);
                }
            }
        }
    }
}
