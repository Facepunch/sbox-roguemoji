using System;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Interfacer;

public abstract class GridPanel : Panel
{
    public virtual int GridWidth => 0;
    public virtual int GridHeight => 0;
    public virtual GridPanelType GridPanelType => GridPanelType.None;

    public readonly List<GridCell> Cells = new List<GridCell>();

    public int HoveredCellIndex { get; set; }
    public int ClickedCellIndex { get; set; }

    public Dictionary<IntVector, List<CellVfx>> CellVfxs = new Dictionary<IntVector, List<CellVfx>>();
    public List<IntVector> GridCellsToRefresh = new List<IntVector>();

    public override void Tick()
    {
        base.Tick();

        float dt = Time.Delta;
        foreach (KeyValuePair<IntVector, List <CellVfx>> pair in CellVfxs)
        {
            var vfxList = pair.Value;

            //if(vfxList.Count > 0)
            //{
            //    var gridCell = GetCell(pair.Key);
            //    Log.Info(gridCell.Text + " vfxList.Count: " + vfxList.Count);
            //}
            
            for(int i = vfxList.Count - 1; i >= 0; i--)
            {
                var vfx = vfxList[i];
                //Log.Info(pair.Key + " i: " + i + " -- " + vfx.GetType().Name + " vfxList.Count: " + vfxList.Count + " ---------- " + Time.Delta);
                vfx.Update(dt);
            }
        }

        RefreshCells();
    }

    public string GetSelectedIndexString()
    {
        if (Hud.Instance.SelectedCell != null)
            return Hud.Instance.SelectedCell.Index.ToString();
        else
            return "None";
    }

    public IntVector GetGridPos(int index)
    {
        return new IntVector(index % GridWidth, MathX.FloorToInt((float)index / (float)GridWidth));
    }
    public int GetIndex(IntVector gridPos)
    {
        return gridPos.y * GridWidth + gridPos.x;
    }

    public Vector2 GetCellPos(int index)
    {
        return new Vector2(index % GridWidth, MathX.FloorToInt((float)index / (float)GridWidth)) * 40f;
    }

    protected void OnCellClicked(int index)
    {
        var gridPos = GridManager.GetGridPos(index, GridWidth);
        Hud.Instance.GridCellClicked(GridPanelType, gridPos.x, gridPos.y);
    }

    protected override void OnChildAdded(Panel child)
    {
        base.OnChildAdded(child);

        if (child is GridCell gridCell)
        {
            Cells.Add(gridCell);
        }
    }

    protected override void OnChildRemoved(Panel child)
    {
        base.OnChildRemoved(child);

        if (child is GridCell gridCell)
        {
            Cells.Remove(gridCell);
        }
    }

    public GridCell GetCell(int index)
    {
        if (index < Cells.Count)
            return Cells[index];

        return null;
    }

    public GridCell GetCell(int x, int y)
    {
        return GetCell(y * GridWidth + x);
    }

    public GridCell GetCell(IntVector gridPos)
    {
        return GetCell(gridPos.y * GridWidth + gridPos.x);
    }

    public CellVfx AddCellVfx(IntVector gridPos, TypeDescription type)
    {
        if (!CellVfxs.ContainsKey(gridPos))
        {
            CellVfxs[gridPos] = new List<CellVfx>();
        }
        else
        {
            foreach (var v in CellVfxs[gridPos])
            {
                var vType = TypeLibrary.GetDescription(v.GetType());
                if(vType == type)
                {
                    Log.Error("AddCellVfx: " + gridPos + " already has vfx of type " + type.Name + "!");
                    return null;
                }
            }
        }

        var gridCell = GetCell(gridPos);

        var vfx = type.Create<CellVfx>();
        vfx.Init(gridCell, gridPos, this);
        CellVfxs[gridPos].Add(vfx);
        return vfx;
    }

    public void RemoveCellVfx(IntVector gridPos, TypeDescription type)
    {
        //Log.Info("RemoveCellVfx: " + gridPos + " - " + type.Name);

        if (CellVfxs.ContainsKey(gridPos))
        {
            var vfxList = CellVfxs[gridPos];

            for(int i = vfxList.Count - 1; i >= 0; i--)
            {
                var vfx = vfxList[i];
                var vfxType = TypeLibrary.GetDescription(vfx.GetType());
                if(vfxType == type)
                {
                    vfx.OnRemove();
                    vfxList.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void ClearAllCellVfx(IntVector gridPos)
    {
        if (CellVfxs.ContainsKey(gridPos))
        {
            foreach(var vfx in CellVfxs[gridPos])
                vfx.OnRemove();
        }

        CellVfxs.Clear();
    }

    public void RefreshGridPos(IntVector gridPos)
    {
        if (!GridCellsToRefresh.Contains(gridPos))
            GridCellsToRefresh.Add(gridPos);
    }

    public void RefreshCells()
    {
        foreach (IntVector gridCell in GridCellsToRefresh)
            RefreshCell(gridCell);

        GridCellsToRefresh.Clear();
    }

    void RefreshCell(IntVector gridPos)
    {
        var gridCell = GetCell(gridPos);

        if (gridCell == null)
            return;

        gridCell.RefreshTransform();
    }

    public List<Thing> GetThings()
    {
        return Entity.All
            .OfType<Thing>()
            .Where(x => x.GridPanelType == GridPanelType)
            .ToList();
    }

    public void Refresh()
    {
        StateHasChanged();
    }
}