using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Interfacer;

public abstract class GridPanel : Panel
{
    public virtual int GridWidth => 0;
    public virtual int GridHeight => 0;
    public virtual GridPanelType GridPanelType => GridPanelType.None;

    public readonly List<GridCell> Cells = new List<GridCell>();

    public int HoveredCellIndex { get; set; }
    public int ClickedCellIndex { get; set; }

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
}