using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Roguemoji;

public class SeenThingData
{
    public string icon;
    public TattooData tattooData;
    public bool hasWieldedThing;
    public string wieldedThingIcon;
    public Vector2 wieldedThingOffset;
    public int wieldedThingFontSize;
    public TattooData wieldedThingTattooData;
    public int zIndex;
    public bool isSolid;
    public int playerNum;
    public float opacity;
    public float wieldedOpacity;
    public bool isVisible;
    public int networkIdent;
    public List<SeenFloaterData> floaterList;
}

public struct SeenFloaterData
{
    public string icon;
    public string text;
    public Vector2 offset;
    public float scale;
    public float opacity;

    public SeenFloaterData(string icon, Vector2 offset, string text, float scale, float opacity)
    {
        this.icon = icon;
        this.offset = offset;
        this.text = text;
        this.scale = scale;
        this.opacity = opacity;
    }
}

public enum PlayerVisionChangeReason { ChangedGridPos, IncreasedSightBlockAmount, DecreasedSightBlockAmount, ChangedInvisibleAmount }

public partial class RoguemojiPlayer : ThingBrain
{
    public HashSet<IntVector> VisibleCells { get; set; } // Client-only
    public Dictionary<LevelId, HashSet<IntVector>> SeenCells { get; set; } // Client-only
    public Dictionary<LevelId, Dictionary<IntVector, List<SeenThingData>>> SeenThings { get; set; } // Client-only

    private HashSet<IntVector> _wasVisible = new HashSet<IntVector>();

    public void RefreshVisibility()
    {
        RefreshVisibilityClient(To.Single(this));
    }

    [ClientRpc]
    void RefreshVisibilityClient()
    {
        if (!SeenCells.ContainsKey(ControlledThing.CurrentLevelId))
            SeenCells.Add(ControlledThing.CurrentLevelId, new HashSet<IntVector>());

        if (!SeenThings.ContainsKey(ControlledThing.CurrentLevelId))
            SeenThings.Add(ControlledThing.CurrentLevelId, new Dictionary<IntVector, List<SeenThingData>>());

        _wasVisible.Clear();
        // add the visible cells before updating visibility
        foreach (var gridPos in VisibleCells)
            _wasVisible.Add(gridPos);

        ComputeVisibility(ControlledThing.GridPos, rangeLimit: ControlledThing.GetStatClamped(StatType.SightDistance));

        // SEE RANDOM CELLS MUTATION
        //var sight = ControlledThing.GetStatClamped(StatType.Sight);
        //int RANGE = 11;
        //if (Game.Random.Int(0, 2) == 0)
        //    SetCellVisible(ControlledThing.GridPos.x + Game.Random.Int(-RANGE, RANGE), ControlledThing.GridPos.y + Math.Min(Game.Random.Int(sight + 1, RANGE), RANGE) * (Game.Random.Int(0, 2) == 0 ? -1 : 1));
        //else
        //    SetCellVisible(ControlledThing.GridPos.x + Math.Min(Game.Random.Int(sight + 1, RANGE), RANGE) * (Game.Random.Int(0, 2) == 0 ? -1 : 1), ControlledThing.GridPos.y + Game.Random.Int(-RANGE, RANGE));

        foreach (var gridPos in VisibleCells.Except(_wasVisible)) // newly visible cells
            ClearSeenThings(gridPos);

        foreach (var gridPos in _wasVisible.Except(VisibleCells)) // newly non-visible cells
            SaveSeenData(gridPos);
    }

    [ClientRpc]
    public void RevealEntireLevelClient()
    {
        if (ControlledThing == null)
            return;

        var level = RoguemojiGame.Instance.GetLevel(ControlledThing.CurrentLevelId);
        var gridManager = level.GridManager;

        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                var gridPos = new IntVector(x, y);
                if (VisibleCells.Contains(gridPos))
                    continue;

                SaveSeenData(gridPos);
            }
        }
    }

    void SaveSeenData(IntVector gridPos)
    {
        SeenCells[ControlledThing.CurrentLevelId].Add(gridPos);

        if (!SeenThings[ControlledThing.CurrentLevelId].ContainsKey(gridPos))
            SeenThings[ControlledThing.CurrentLevelId][gridPos] = new List<SeenThingData>();

        SeenThings[ControlledThing.CurrentLevelId][gridPos].Clear();

        var things = ControlledThing.ContainingGridManager.GetThingsAtClient(gridPos).OrderBy(x => x.GetZPos());
        foreach (var thing in things)
        {
            if(!ControlledThing.CanPerceiveAnyPartOfThing(thing))
                continue;

            bool isVisible = ControlledThing.CanPerceiveThing(thing);

            var seenData = new SeenThingData()
            {
                icon = thing.DisplayIcon,
                zIndex = thing.GetZPos(),
                hasWieldedThing = thing.WieldedThing != null && ControlledThing.CanPerceiveThing(thing.WieldedThing),
                isSolid = thing.HasFlag(ThingFlags.Solid),
                playerNum = thing.PlayerNum,
                opacity = isVisible ? (thing.Opacity * (thing.GetStatClamped(StatType.Invisible) > 0 ? 0.5f : 1f)) : 1f, // opacity->1f when invisible (won't be rendered anyway) so opacity doesn't effect wielded thing
                wieldedOpacity = thing.WieldedThing != null ? (thing.WieldedThing.Opacity * (thing.WieldedThing.GetStatClamped(StatType.Invisible) > 0 ? 0.5f : 1f)) : 0f,
                isVisible = isVisible,
                networkIdent = thing.NetworkIdent,
            };

            if (thing.HasTattoo)
            {
                var existingTattoo = thing.TattooData;
                var copiedTattoo = new TattooData()
                {
                    Icon = Hud.GetTattooIcon(thing),
                    Scale = existingTattoo.Scale,
                    Offset = existingTattoo.Offset,
                };

                seenData.tattooData = copiedTattoo;
            }

            if (thing.WieldedThing != null)
            {
                seenData.wieldedThingIcon = thing.WieldedThing.DisplayIcon;

                if (thing.WieldedThing.HasTattoo)
                {
                    var existingWieldedTattoo = thing.WieldedThing.TattooData;
                    var copiedWieldedTattoo = new TattooData()
                    {
                        Icon = Hud.GetTattooIcon(thing.WieldedThing),
                        Scale = existingWieldedTattoo.Scale,
                        OffsetWielded = existingWieldedTattoo.OffsetWielded,
                    };

                    seenData.wieldedThingTattooData = copiedWieldedTattoo;
                }

                seenData.wieldedThingOffset = thing.WieldedThingOffset;
                seenData.wieldedThingFontSize = thing.WieldedThingFontSize;
            }

            if(thing.HasFloaters)
            {
                foreach (var floater in thing.Floaters)
                {
                    if(floater.showOnSeen)
                    {
                        if (seenData.floaterList == null)
                            seenData.floaterList = new List<SeenFloaterData>();

                        var offset = floater.time > 0f ? Vector2.Lerp(floater.offsetStart, floater.offsetEnd, Utils.Map(floater.timeSinceStart, 0f, floater.time, 0f, 1f, floater.offsetEasingType)) : floater.offsetStart;
                        var opacity = Thing.GetFloaterOpacity(floater);
                        seenData.floaterList.Add(new SeenFloaterData(floater.icon, offset, floater.text, floater.scale, opacity));
                    }
                }
            } 

            SeenThings[ControlledThing.CurrentLevelId][gridPos].Add(seenData);
        }
    }

    void ClearSeenThings(IntVector gridPos)
    {
        if (!SeenThings.ContainsKey(ControlledThing.CurrentLevelId))
            return;

        if (!SeenThings[ControlledThing.CurrentLevelId].ContainsKey(gridPos))
            return;

        SeenThings[ControlledThing.CurrentLevelId][gridPos].Clear();
    }

    public void CheckForUnnecessarySeenThing(Thing thing)
    {
        if (!SeenThings.ContainsKey(ControlledThing.CurrentLevelId))
            return;

        var gridThings = SeenThings[ControlledThing.CurrentLevelId];
        foreach(KeyValuePair<IntVector, List<SeenThingData>> pair in gridThings)
        {
            var things = pair.Value;
            for(int i = things.Count - 1; i >= 0; i--)
            {
                if(thing.Equals(things[i]))
                {
                    things.RemoveAt(i);
                }
            }
        }
    }

    [ClientRpc]
    public void CheckForUnnecessarySeenThings()
    {
        if (!SeenThings.ContainsKey(ControlledThing.CurrentLevelId))
            return;

        var gridThings = SeenThings[ControlledThing.CurrentLevelId];
        foreach (KeyValuePair<IntVector, List<SeenThingData>> pair in gridThings)
        {
            var things = pair.Value;
            for (int i = things.Count - 1; i >= 0; i--)
            {
                var data = things[i];
                Thing thing = FindByIndex(data.networkIdent) as Thing;

                if (thing == null)
                    continue;

                if (IsCellVisible(thing.GridPos) && ControlledThing.CanPerceiveThing(thing))
                    things.RemoveAt(i);
            }
        }
    }

    public bool IsCellVisible(IntVector gridPos)
    {
        return VisibleCells.Contains(gridPos);
    }

    public bool IsCellSeen(IntVector gridPos)
    {
        return SeenCells.ContainsKey(ControlledThing.CurrentLevelId) && SeenCells[ControlledThing.CurrentLevelId].Contains(gridPos);
    }

    public List<SeenThingData> GetSeenThings(IntVector gridPos)
    {
        if(SeenThings[ControlledThing.CurrentLevelId].ContainsKey(gridPos))
            return SeenThings[ControlledThing.CurrentLevelId][gridPos];

        return null;
    }

    public void SetCellVisible(int x, int y)
    {
        IntVector gridPos = new IntVector(x, y);

        if (!ControlledThing.ContainingGridManager.IsGridPosInBounds(gridPos))
            return;

        VisibleCells.Add(gridPos);
    }

    public void ComputeVisibility(IntVector origin, int rangeLimit)
    {
        VisibleCells.Clear();

        SetCellVisible(origin.x, origin.y);
        for (uint octant = 0; octant < 8; octant++) 
            Compute(octant, origin, rangeLimit, 1, new Slope(1, 1), new Slope(0, 1));
    }

    struct Slope // represents the slope Y/X as a rational number
    {
        public Slope(uint y, uint x) { Y = y; X = x; }

        public bool Greater(uint y, uint x) { return Y * x > X * y; } // this > y/x
        public bool GreaterOrEqual(uint y, uint x) { return Y * x >= X * y; } // this >= y/x
        public bool Less(uint y, uint x) { return Y * x < X * y; } // this < y/x
        public bool LessOrEqual(uint y, uint x) { return Y * x <= X * y; } // this <= y/x

        public readonly uint X, Y;
    }

    // http://www.adammil.net/blog/v125_roguelike_vision_algorithms.html#mycode
    void Compute(uint octant, IntVector origin, int rangeLimit, uint x, Slope top, Slope bottom)
    {
        for (; x <= (uint)rangeLimit; x++)
        {
            uint topY;
            if (top.X == 1)
            {
                topY = x;
            }
            else
            {
                topY = ((x * 2 - 1) * top.Y + top.X) / (top.X * 2); 
                if (BlocksLight(x, topY, octant, origin))
                {
                    if (top.GreaterOrEqual(topY * 2 + 1, x * 2) && !BlocksLight(x, topY + 1, octant, origin)) topY++;
                }
                else 
                {
                    uint ax = x * 2; // center
                    if (BlocksLight(x + 1, topY + 1, octant, origin)) ax++; // use bottom-right if the tile above and right is a wall
                    if (top.Greater(topY * 2 + 1, ax)) topY++;
                }
            }

            uint bottomY;
            if (bottom.Y == 0)
            {                 
                bottomY = 0;
            }
            else // bottom > 0
            {
                bottomY = ((x * 2 - 1) * bottom.Y + bottom.X) / (bottom.X * 2);
                if (bottom.GreaterOrEqual(bottomY * 2 + 1, x * 2) && BlocksLight(x, bottomY, octant, origin) &&
                   !BlocksLight(x, bottomY + 1, octant, origin))
                {
                    bottomY++;
                }
            }

            // go through the tiles in the column now that we know which ones could possibly be visible
            int wasOpaque = -1; // 0:false, 1:true, -1:not applicable
            for (uint y = topY; (int)y >= (int)bottomY; y--)
            {
                if (rangeLimit < 0 || Utils.GetDistance((int)x, (int)y) <= rangeLimit) // skip the tile if it's out of visual range
                {
                    bool isOpaque = BlocksLight(x, y, octant, origin);

                    bool isVisible = (y != topY || top.GreaterOrEqual(y, x)) && (y != bottomY || bottom.LessOrEqual(y, x));
                    
                    if (isVisible) 
                        SetVisible(x, y, octant, origin);

                    if (x != rangeLimit) 
                    {
                        if (isOpaque)
                        {
                            if (wasOpaque == 0) 
                            {                  
                                uint nx = x * 2, ny = y * 2 + 1; 
                                                                 
                                // NOTE: if you're using full symmetry and want more expansive walls (recommended), comment out the next line
                                //if (BlocksLight(x, y + 1, octant, origin)) nx--;

                                if (top.Greater(ny, nx)) 
                                {                       
                                    if (y == bottomY) 
                                    { 
                                        bottom = new Slope(ny, nx); 
                                        break; 
                                    } 
                                    else
                                    {
                                        Compute(octant, origin, rangeLimit, x + 1, top, new Slope(ny, nx));
                                    }
                                }
                                else 
                                {    
                                    if (y == bottomY) 
                                        return;
                                }
                            }
                            wasOpaque = 1;
                        }
                        else
                        {
                            if (wasOpaque > 0) 
                            {
                                uint nx = x * 2, ny = y * 2 + 1; 
                                                                 
                                // NOTE: if you're using full symmetry and want more expansive walls (recommended), comment out the next line
                                //if (BlocksLight(x + 1, y + 1, octant, origin)) nx++;
                                                                                     
                                if (bottom.GreaterOrEqual(ny, nx)) 
                                    return;

                                top = new Slope(ny, nx);
                            }
                            wasOpaque = 0;
                        }
                    }
                }
            }

            if (wasOpaque != 0) 
                break;
        }
    }

    // NOTE: the code duplication between BlocksLight and SetVisible is for performance. don't refactor the octant
    // translation out unless you don't mind an 18% drop in speed
    bool BlocksLight(uint x, uint y, uint octant, IntVector origin)
    {
        uint nx = (uint)origin.x, ny = (uint)origin.y;
        switch (octant)
        {
            case 0: nx += x; ny -= y; break;
            case 1: nx += y; ny -= x; break;
            case 2: nx -= y; ny -= x; break;
            case 3: nx -= x; ny -= y; break;
            case 4: nx -= x; ny += y; break;
            case 5: nx -= y; ny += x; break;
            case 6: nx += y; ny += x; break;
            case 7: nx += x; ny += y; break;
        }
        return BlocksLight((int)nx, (int)ny);
    }

    void SetVisible(uint x, uint y, uint octant, IntVector origin)
    {
        uint nx = (uint)origin.x, ny = (uint)origin.y;
        switch (octant)
        {
            case 0: nx += x; ny -= y; break;
            case 1: nx += y; ny -= x; break;
            case 2: nx -= y; ny -= x; break;
            case 3: nx -= x; ny -= y; break;
            case 4: nx -= x; ny += y; break;
            case 5: nx -= y; ny += x; break;
            case 6: nx += y; ny += x; break;
            case 7: nx += x; ny += y; break;
        }
        SetCellVisible((int)nx, (int)ny);
    }

    bool BlocksLight(int x, int y)
    {
        Game.AssertClient();

        return ControlledThing.ContainingGridManager.GetThingsAtClient(new IntVector(x, y)).Where(x => x.GetStatClamped(StatType.SightBlockAmount) >= ControlledThing.GetStatClamped(StatType.SightPenetration)).Where(x => x.GetStatClamped(StatType.Invisible) <= 0).Count() > 0;
    }
}
