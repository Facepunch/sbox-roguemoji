using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using static Roguemoji.DebugDrawing;

namespace Roguemoji;

public enum PanelType { None, ArenaGrid, InventoryGrid, EquipmentGrid, Wielding, PlayerIcon, Log, Nearby, Info, Character, Stats };

public class FloaterData
{
    public string icon { get; set; }
    public IntVector gridPos { get; set; }
    public float time { get; set; }
    public float elapsedTime { get; set; }
    public string text { get; set; }
    public bool requireSight { get; set; }
    public float yOffsetStart { get; set; }
    public float yOffsetEnd { get; set; }
    public EasingType offsetEasingType { get; set; }

    public FloaterData(string icon, IntVector gridPos, float time, string text, bool requireSight, float yOffsetStart, float yOffsetEnd, EasingType offsetEasingType)
    {
        this.icon = icon;
        this.gridPos = gridPos;
        this.time = time;
        this.elapsedTime = 0f;
        this.text = text;
        this.requireSight = requireSight;
        this.yOffsetStart = yOffsetStart;
        this.yOffsetEnd = yOffsetEnd;
        this.offsetEasingType = offsetEasingType;
    }
}

public partial class Hud : RootPanel
{
	public static Hud Instance { get; private set; }

	public int IndexClicked { get; private set; }

	public GridCell SelectedCell { get; private set; }

	public MainPanel MainPanel { get; private set; }
    public DebugDrawing DebugDrawing { get; set; }
    public FloaterDisplay FloaterDisplay { get; set; }

    public bool IsDraggingThing { get; set; }
    public bool IsDraggingRightClick { get; set; }
    public Thing DraggedThing { get; set; }
    public Panel DraggedPanel { get; set; }
    public DragIcon DragIcon { get; private set; }
	private IntVector _dragStartPlayerGridPos;

	public Vector2 GetMousePos() { return MousePosition / ScaleToScreen; }

    public List<FloaterData> Floaters { get; private set; } = new List<FloaterData>();

    public Hud()
	{
		Instance = this;

		StyleSheet.Load("/ui/Hud.scss");

		MainPanel = AddChild<MainPanel>();
        DebugDrawing = AddChild<DebugDrawing>();
        FloaterDisplay = AddChild<FloaterDisplay>();
    }

    public override void Tick()
    {
        base.Tick();

        var dt = Time.Delta;

		// if dragging a nearby thing, stop when moving
		if(IsDraggingThing && !_dragStartPlayerGridPos.Equals(RoguemojiGame.Instance.LocalPlayer.GridPos))
		{
			if(DraggedThing != null && DraggedThing.ContainingGridManager.GridType == GridType.Arena)
				StopDragging();
		}

        //      var player = RoguemojiGame.Instance.LocalPlayer;
        //foreach (var gridPos in player.VisibleCells)
        //{
        //	//DebugDrawing.GridCell(gridPos, new Color(0f, 0f, 1f, 0.1f));
        //}
        //DebugDrawing.Line(GetScreenPosForArenaGridPos(new IntVector(18, 19)), GetScreenPosForArenaGridPos(new IntVector(21, 22)), new Color(0f, 1f, 1f, 0.9f));
        //DebugDrawing.GridLine(new IntVector(19, 19), new IntVector(22, 22), Color.Blue);
        //DebugDrawing.GridCell(new IntVector(20, 20), Color.Red);

        for(int i = Floaters.Count - 1; i >= 0; i--)
        {
            var floater = Floaters[i];

            //DebugDrawing.GridLine(IntVector.Zero, floater.gridPos, new Color(0f, 1f, 0f, 0.5f));

            floater.elapsedTime += dt;
            if(floater.elapsedTime > floater.time)
                Floaters.RemoveAt(i);
        }
	}

    public void GridCellClicked(IntVector gridPos, GridType gridType, bool rightClick, bool shift, bool doubleClick, bool visible = true)
	{
        AddFloater("💔", gridPos, 0.75f, "-4", requireSight: true, 1f, -6f, EasingType.SineIn);

		RoguemojiGame.GridCellClickedCmd(gridPos.x, gridPos.y, gridType, rightClick, shift, doubleClick, visible);
	}

    public void WieldingClicked(bool rightClick, bool shift)
    {
        RoguemojiGame.WieldingClickedCmd(rightClick, shift);
    }

    public void PlayerIconClicked(bool rightClick, bool shift)
    {
        RoguemojiGame.PlayerIconClickedCmd(rightClick, shift);
    }

    protected override void OnMouseUp(MousePanelEvent e)
    {
        base.OnMouseUp(e);

		if(IsDraggingThing)
		{
			PanelType destinationPanelType = GetContainingPanelType(MousePosition);

			IntVector targetGridPos = IntVector.Zero;
			var gridType = GetGridType(destinationPanelType);
            if (gridType != GridType.None)
			{
				GridPanel gridPanel = GetGridPanel(gridType);
				targetGridPos = gridPanel.GetGridPos(gridPanel.MousePosition);

                var player = RoguemojiGame.Instance.LocalPlayer;
				var gridManager = player.GetGridManager(gridType);
				if (!gridManager.IsGridPosInBounds(targetGridPos)) 
				{
                    StopDragging();
					return;
                }
			}

			if(DraggedThing.ContainingGridManager.GridType == GridType.Inventory)
				RoguemojiGame.InventoryThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);
			else if (DraggedThing.ContainingGridManager.GridType == GridType.Equipment)
                RoguemojiGame.EquipmentThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);
            else
				RoguemojiGame.NearbyThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);

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
        else if (Contains(GetRect(PanelType.PlayerIcon), pos))
            return PanelType.PlayerIcon;
        else if (Contains(GetRect(PanelType.Character), pos))
            return PanelType.Character;

        return PanelType.None;
	}

	public Rect GetRect(PanelType panelType)
	{
		return GetPanel(panelType)?.Box.Rect ?? new Rect();
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
            case PanelType.PlayerIcon:
                return MainPanel.CharacterPanel.PlayerIcon;
        }

		return null;
	}

    public GridPanel GetGridPanel(GridType gridType)
    {
        Game.AssertClient();

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

	public GridType GetGridType(PanelType panelType)
    {
		switch(panelType)
		{
            case PanelType.ArenaGrid:
                return GridType.Arena;
            case PanelType.InventoryGrid:
				return GridType.Inventory; ;
            case PanelType.EquipmentGrid:
                return GridType.Equipment;
        }

		return GridType.None;
    }

    bool Contains(Rect rect, Vector2 point)
	{
		return point.x > rect.Left && point.x < rect.Right && point.y > rect.Top && point.y < rect.Bottom;
	}

    public Vector2 GetScreenPosForArenaGridPos(IntVector gridPos)
    {
        var player = RoguemojiGame.Instance.LocalPlayer;
        var arenaPanel = GetGridPanel(GridType.Arena);
        var rect = GetRect(PanelType.ArenaGrid);

        if (arenaPanel == null)
            return Vector2.Zero;

        //Log.Info($"GetScreenPosForArenaGridPos: {gridPos} - {(rect.TopLeft + arenaPanel.GetCellPos(gridPos - player.CameraGridOffset) + player.CameraPixelOffset)}");

        return rect.TopLeft + arenaPanel.GetCellPos(gridPos - player.CameraGridOffset) + player.CameraPixelOffset;
    }

    //public Vector2 GetScreenPosForPos(Vector2 pos)
    //{
    //    var player = RoguemojiGame.Instance.LocalPlayer;
    //    var rect = GetRect(PanelType.ArenaGrid);

    //    return rect.TopLeft + pos - player.CameraGridOffset * (40f / ScaleFromScreen) + player.CameraPixelOffset;
    //}

    public void AddFloater(string icon, IntVector gridPos, float time, string text = "", bool requireSight = true, float yOffsetStart = 0f, float yOffsetEnd = 0f, EasingType offsetEasingType = EasingType.Linear)
    {
        Floaters.Add(new FloaterData(icon, gridPos, time, text, requireSight, yOffsetStart, yOffsetEnd, offsetEasingType));
    }

    public void Restart()
    {
        Floaters.Clear();
    }
}
