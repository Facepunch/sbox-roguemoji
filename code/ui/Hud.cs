using Sandbox;
using Sandbox.UI;
using System.Diagnostics.SymbolStore;

namespace Interfacer;

public enum PanelType { None, ArenaGrid, InventoryGrid, Log, Nearby, Info, Character, Stats };

public partial class Hud : RootPanel
{
	public static Hud Instance { get; private set; }

	public int IndexClicked { get; private set; }

	public GridCell SelectedCell { get; private set; }

	public MainPanel MainPanel { get; private set; }

	public bool IsDraggingThing { get; set; }
	public Thing DraggedThing { get; set; }
    public DragIcon DragIcon { get; private set; }

	public Vector2 GetMousePos() { return MousePosition / ScaleToScreen; }

    public Hud()
	{
		Instance = this;

		StyleSheet.Load("/ui/Hud.scss");

		MainPanel = AddChild<MainPanel>();
	}

    //public override void Tick()
    //{
    //    base.Tick();

    //    DebugOverlay.ScreenText("dragging: " + (Instance.IsDraggingThing ? DraggedThing.DisplayName : ""), Instance.MousePosition);
    //}

    public void GridCellClickedArena(IntVector gridPos, bool rightClick, bool shift)
	{
		InterfacerGame.CellClickedArenaCmd(gridPos.x, gridPos.y, rightClick, shift);
	}

	public void GridCellClickedInventory(IntVector gridPos, bool rightClick, bool shift)
	{
		InterfacerGame.CellClickedInventoryCmd(gridPos.x, gridPos.y, rightClick, shift);
	}

    //protected override void OnMouseDown(MousePanelEvent e)
    //{
    //    base.OnMouseDown(e);

    //    //Log.Info("Hud:OnMouseDown");
    //}

    protected override void OnMouseUp(MousePanelEvent e)
    {
        base.OnMouseUp(e);

		if(IsDraggingThing)
		{
			PanelType destinationPanelType = GetContainingPanelType(MousePosition);

			IntVector targetGridPos = IntVector.Zero;
			if(destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.InventoryGrid)
			{
				GridPanel gridPanel = GetPanel(destinationPanelType) as GridPanel;
				targetGridPos = gridPanel.GetGridPos(gridPanel.MousePosition);
			}

			if(DraggedThing.Flags.HasFlag(ThingFlags.InInventory))
			{
				InterfacerGame.InventoryThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);
            }
			else
			{
				InterfacerGame.NearbyThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);
			}

            StopDragging();
		}
    }

	public void StartDragging(Thing thing)
	{
		IsDraggingThing = true;
		DraggedThing = thing;

		CreateDragIcon(thing.DisplayIcon);
	}

	public void StopDragging() 
	{
		IsDraggingThing = false;
		DraggedThing = null;

		RemoveDragIcon();
	}

	void CreateDragIcon(string icon)
	{
		RemoveDragIcon();
        DragIcon = AddChild<DragIcon>();
		DragIcon.Text = icon;
    }

	void RemoveDragIcon()
	{
        if (DragIcon != null)
            DragIcon.Delete();
    }

	public PanelType GetContainingPanelType(Vector2 pos)
	{
		if (Contains(GetRect(PanelType.ArenaGrid), pos))
			return PanelType.ArenaGrid;
		else if (Contains(GetRect(PanelType.InventoryGrid), pos))
			return PanelType.InventoryGrid;
		else if (Contains(GetRect(PanelType.Nearby), pos))
			return PanelType.Nearby;
        else if (Contains(GetRect(PanelType.Log), pos))
            return PanelType.Log;

        return PanelType.None;
	}

	public Rect GetRect(PanelType panelType)
	{
		return GetPanel(panelType).Box.Rect;
	}

	public Panel GetPanel(PanelType panelType)
	{
		switch(panelType)
		{
			case PanelType.ArenaGrid:
				return MainPanel.ArenaPanel;
			case PanelType.InventoryGrid:
                return MainPanel.InventoryPanel;
			case PanelType.Log:
                return MainPanel.LogPanel;
			case PanelType.Nearby:
                return MainPanel.NearbyPanel;
        }

		return null;
	}

	bool Contains(Rect rect, Vector2 point)
	{
		return point.x > rect.Left && point.x < rect.Right && point.y > rect.Top && point.y < rect.Bottom;
	}
}
