using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class RoguemojiPlayer : Thing
{
    public HashSet<IntVector> VisibleCells { get; set; } // Client-only

    [ClientRpc]
    public void RefreshVisibility()
    {
        ComputeVisibility(GridPos, rangeLimit: GetStatClamped(StatType.Sight));
    }

    public bool IsCellVisible(IntVector gridPos)
    {
        return VisibleCells.Contains(gridPos);
    }

    public void SetCellVisible(int x, int y)
    {
        IntVector gridPos = new IntVector(x, y);

        if (!ContainingGridManager.IsGridPosInBounds(gridPos))
            return;

        VisibleCells.Add(gridPos);
    }

    public void ClearCellVisibility()
    {
        VisibleCells.Clear();
    }

    public void ComputeVisibility(IntVector origin, int rangeLimit)
    {
        ClearCellVisibility();

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
                if (rangeLimit < 0 || GetDistance((int)x, (int)y) <= rangeLimit) // skip the tile if it's out of visual range
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

    int GetDistance(int x, int y)
    {
        return (int)Math.Round(Math.Sqrt(x * x + y * y));
    }
    
    bool BlocksLight(int x, int y)
    {
        Game.AssertClient();

        return ContainingGridManager.GetThingsAtClient(new IntVector(x, y)).Where(x => x.SightBlockAmount >= GetStatClamped(StatType.Sight)).Count() > 0;
    }
}
