using System;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Interfacer;

public abstract class GridPanel : Panel
{
    public virtual int GridWidth => 0;
    public virtual int GridHeight => 0;

    public int HoveredCellIndex { get; set; }
    public int ClickedCellIndex { get; set; }

    public string GetSelectedIndexString()
    {
        if (Hud.Instance.SelectedCell != null)
            return Hud.Instance.SelectedCell.GridIndex.ToString();
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

    public Vector2 GetCellPos(IntVector gridPos)
    {
        return PanelPositionToScreenPosition(new Vector2(gridPos.x, gridPos.y) * 40f / ScaleFromScreen);
    }

    public IntVector GetGridPos(Vector2 screenPos)
    {
        float cellSize = 40f / ScaleFromScreen;
        return new IntVector(MathX.FloorToInt(screenPos.x / cellSize), MathX.FloorToInt(screenPos.y / cellSize));
    }

    protected virtual void OnThingClicked(int index)
    {
        
    }

    protected virtual void OnBgClicked()
    {
        
    }

    protected virtual IList<Thing> GetThings()
    {
        return null;
    }

    protected override int BuildHash()
    {
        return HashCode.Combine(GetThings().Count);
    }

    public void Refresh()
    {
        StateHasChanged();
    }
}