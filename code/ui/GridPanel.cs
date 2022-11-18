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

    public int HoveredCellIndex { get; set; }
    public int ClickedCellIndex { get; set; }

    public override void Tick()
    {
        base.Tick();

        //Log.Info(GridPanelType + " --- hovered: " + GetGridPos(HoveredCellIndex) + " clicked: " + GetGridPos(ClickedCellIndex));
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

    public Vector2 GetCellPos(IntVector gridPos)
    {
        return PanelPositionToScreenPosition(new Vector2(gridPos.x, gridPos.y) * 40f / ScaleFromScreen);
    }

    public IntVector GetGridPos(Vector2 screenPos)
    {
        float cellSize = 40f / ScaleFromScreen;
        return new IntVector(MathX.FloorToInt(screenPos.x / cellSize), MathX.FloorToInt(screenPos.y / cellSize));
    }

    protected void OnThingClicked(int index)
    {
        var gridPos = GridManager.GetGridPos(index, GridWidth);
        Hud.Instance.GridCellClicked(GridPanelType, gridPos);
    }
    protected void OnBgClicked()
    {
        Hud.Instance.GridCellClicked(GridPanelType, GetGridPos(MousePosition));
    }

    public IList<Thing> GetThings()
    {
        return ThingManager.Instance.GetThings(GridPanelType);
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