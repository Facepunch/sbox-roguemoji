using Sandbox;
using Sandbox.UI;
using System.Diagnostics.SymbolStore;

namespace Roguemoji;

public enum PanelType { None, ArenaGrid, InventoryGrid, EquipmentGrid, Wielding, Log, Nearby, Info, Character, Stats };

public partial class Hud : RootPanel
{
	public static Hud Instance { get; private set; }

	public int IndexClicked { get; private set; }

	public GridCell SelectedCell { get; private set; }

	public MainPanel MainPanel { get; private set; }

	public bool IsDraggingThing { get; set; }
    public bool IsDraggingRightClick { get; set; }
    public Thing DraggedThing { get; set; }
    public Panel DraggedPanel { get; set; }
    public DragIcon DragIcon { get; private set; }
	private IntVector _dragStartPlayerGridPos;

	public Vector2 GetMousePos() { return MousePosition / ScaleToScreen; }

    public Hud()
	{
		Instance = this;

		StyleSheet.Load("/ui/Hud.scss");

		MainPanel = AddChild<MainPanel>();
	}

    public override void Tick()
    {
        base.Tick();

		// if dragging a nearby thing, stop when moving
		if(IsDraggingThing && !_dragStartPlayerGridPos.Equals(RoguemojiGame.Instance.LocalPlayer.GridPos))
		{
			if(DraggedThing != null && DraggedThing.ContainingGridManager.GridType == GridType.Arena)
			{
				StopDragging();
			}
		}
    }

    public void GridCellClickedArena(IntVector gridPos, bool rightClick, bool shift)
	{
		RoguemojiGame.CellClickedArenaCmd(gridPos.x, gridPos.y, rightClick, shift);
	}

	public void GridCellClickedInventory(IntVector gridPos, bool rightClick, bool shift)
	{
        Log.Info("Hud:GridCellClickedInventory: " + gridPos);
        RoguemojiGame.CellClickedInventoryCmd(gridPos.x, gridPos.y, rightClick, shift);
	}

    public void GridCellClickedEquipment(IntVector gridPos, bool rightClick, bool shift)
    {
		Log.Info("Hud:GridCellClickedEquipment: " + gridPos);
        RoguemojiGame.CellClickedEquipmentCmd(gridPos.x, gridPos.y, rightClick, shift);
    }

    protected override void OnMouseUp(MousePanelEvent e)
    {
        base.OnMouseUp(e);

		if(IsDraggingThing)
		{
			PanelType destinationPanelType = GetContainingPanelType(MousePosition);

			IntVector targetGridPos = IntVector.Zero;
			if(destinationPanelType == PanelType.ArenaGrid || destinationPanelType == PanelType.InventoryGrid || destinationPanelType == PanelType.EquipmentGrid)
			{
				GridPanel gridPanel = GetPanel(destinationPanelType) as GridPanel;
				targetGridPos = gridPanel.GetGridPos(gridPanel.MousePosition);
			}

			Log.Info(DraggedThing.DisplayIcon + " DraggedThing.ContainingGridManager.GridType: " + DraggedThing.ContainingGridManager.GridType);

			if(DraggedThing.ContainingGridManager.GridType == GridType.Inventory)
			{
				RoguemojiGame.InventoryThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);
            }
			else if (DraggedThing.ContainingGridManager.GridType == GridType.Equipment)
            {
                RoguemojiGame.EquipmentThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);
            }
            else
			{
				RoguemojiGame.NearbyThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);
			}

            StopDragging();
		}
    }

	public void StartDragging(Thing thing, Panel panel, bool rightClick)
	{
		IsDraggingThing = true;
		IsDraggingRightClick = rightClick;
		DraggedThing = thing;
		DraggedPanel = panel;
		_dragStartPlayerGridPos = RoguemojiGame.Instance.LocalPlayer.GridPos;

        CreateDragIcon(thing);
	}

	public void StopDragging() 
	{
		IsDraggingThing = false;
		DraggedThing = null;

		if(DraggedPanel != null)
		{
			DraggedPanel.SkipTransitions();
			DraggedPanel = null;
		}

		RemoveDragIcon();
	}

	void CreateDragIcon(Thing thing)
	{
		RemoveDragIcon();
        DragIcon = AddChild<DragIcon>();
		DragIcon.Thing = thing;
		DragIcon.Text = thing.DisplayIcon;
    }

	void RemoveDragIcon()
	{
        if (DragIcon != null)
		{
			DragIcon.SkipTransitions();
            DragIcon.Delete();
        }
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
        else if (Contains(GetRect(PanelType.EquipmentGrid), pos))
            return PanelType.EquipmentGrid;
        else if (Contains(GetRect(PanelType.Wielding), pos))
            return PanelType.Wielding;
        else if (Contains(GetRect(PanelType.Character), pos))
            return PanelType.Character;

        return PanelType.None;
	}

	public Rect GetRect(PanelType panelType)
	{
		return GetPanel(panelType).Box.Rect;
	}

	public Panel GetPanel(PanelType panelType)
	{
        switch (panelType)
		{
			case PanelType.ArenaGrid:
				return MainPanel.ArenaPanel;
			case PanelType.InventoryGrid:
                return MainPanel.InventoryPanel;
			case PanelType.Log:
                return MainPanel.LogPanel;
			case PanelType.Nearby:
                return MainPanel.NearbyPanel;
			case PanelType.Character:
				return MainPanel.CharacterPanel;
            case PanelType.EquipmentGrid:
                return MainPanel.CharacterPanel.EquipmentPanel;
            case PanelType.Wielding:
                return MainPanel.CharacterPanel.WieldingPanel;
        }

		return null;
	}

    public GridPanel GetGridPanel(GridType gridType)
    {
        Host.AssertClient();

        switch (gridType)
        {
            case GridType.Arena:
                return MainPanel?.ArenaPanel ?? null;
            case GridType.Inventory:
                return MainPanel?.InventoryPanel ?? null;
            case GridType.Equipment:
                return MainPanel.CharacterPanel?.EquipmentPanel ?? null;
        }

		return null;
    }

    bool Contains(Rect rect, Vector2 point)
	{
		return point.x > rect.Left && point.x < rect.Right && point.y > rect.Top && point.y < rect.Bottom;
	}
}
