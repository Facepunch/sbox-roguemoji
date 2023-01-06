using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using static Roguemoji.DebugDrawing;

namespace Roguemoji;

public enum PanelType { None, ArenaGrid, InventoryGrid, EquipmentGrid, Wielding, PlayerIcon, Log, Nearby, Info, Character, Stats, Chatbox, LevelLabel };
public enum CursorMode { Point, Pinch, Invalid, Write, PointDown, ThumbsUp, Ok, Check, MiddleFinger, Point2, PointLeft, PointRight }

public struct FloaterData
{
    public string icon;
    public IntVector gridPos;
    public float time;
    public TimeSince timeSinceStart;
    public string text;
    public bool requireSight;
    public Vector2 offsetStart;
    public Vector2 offsetEnd;
    public EasingType offsetEasingType;
    public float fadeInTime;
    public float scale;
    public Thing parent;

    public FloaterData(string icon, IntVector gridPos, float time, Vector2 offsetStart, Vector2 offsetEnd, string text, bool requireSight, EasingType offsetEasingType, float fadeInTime, float scale, Thing parent)
    {
        this.icon = icon;
        this.gridPos = gridPos;
        this.time = time;
        this.timeSinceStart = 0f;
        this.offsetStart = offsetStart;
        this.offsetEnd = offsetEnd;
        this.text = text;
        this.requireSight = requireSight;
        this.offsetEasingType = offsetEasingType;
        this.fadeInTime = fadeInTime;
        this.scale = scale;
        this.parent = parent;
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
    public CursorDisplay CursorDisplay { get; set; }

    public bool IsDraggingThing { get; set; }
    public bool IsDraggingRightClick { get; set; }
    public Thing DraggedThing { get; set; }
    public Panel DraggedPanel { get; set; }
    public PanelType DraggedPanelType { get; set; }
    public DragIcon DragIcon { get; private set; }
    public TimeSince TimeSinceStartDragging { get; private set; }
    public Vector2 DragStartPosition { get; private set; }
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
        CursorDisplay = AddChild<CursorDisplay>();
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

            if(floater.timeSinceStart > floater.time)
                Floaters.RemoveAt(i);
        }
	}

    public void GridCellClicked(IntVector gridPos, GridType gridType, bool rightClick, bool shift, bool doubleClick, bool visible = true)
	{
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

    public void UnfocusChatbox()
    {
        MainPanel.Chatbox.Unfocus();
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

            if(destinationPanelType == PanelType.Chatbox || destinationPanelType == PanelType.Log)
            {
                MainPanel.Chatbox.AddIcon(DraggedThing.ChatDisplayIcons);
                StopDragging();
                return;
            }

            if (DraggedThing.ContainingGridManager.GridType == GridType.Inventory)
				RoguemojiGame.InventoryThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y, wieldedThingDragged: DraggedPanelType == PanelType.Wielding);
			else if (DraggedThing.ContainingGridManager.GridType == GridType.Equipment)
                RoguemojiGame.EquipmentThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);
            else
				RoguemojiGame.NearbyThingDraggedCmd(DraggedThing.NetworkIdent, destinationPanelType, targetGridPos.x, targetGridPos.y);

            StopDragging();
		}
    }

	public void StartDragging(Thing thing, Panel panel, bool rightClick, PanelType draggedPanelType)
	{
		IsDraggingThing = true;
		IsDraggingRightClick = rightClick;
		DraggedThing = thing;
		DraggedPanel = panel;
        DraggedPanelType = draggedPanelType;
		_dragStartPlayerGridPos = RoguemojiGame.Instance.LocalPlayer.GridPos;
        TimeSinceStartDragging = 0f;
        DragStartPosition = MousePosition;

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
        if (Contains(GetRect(PanelType.LevelLabel), pos))           return PanelType.LevelLabel;
        else if (Contains(GetRect(PanelType.ArenaGrid), pos))       return PanelType.ArenaGrid;
		else if (Contains(GetRect(PanelType.InventoryGrid), pos))   return PanelType.InventoryGrid;
		else if (Contains(GetRect(PanelType.Nearby), pos))          return PanelType.Nearby;
        else if (Contains(GetRect(PanelType.Log), pos))             return PanelType.Log;
        else if (Contains(GetRect(PanelType.EquipmentGrid), pos))   return PanelType.EquipmentGrid;
        else if (Contains(GetRect(PanelType.Wielding), pos))        return PanelType.Wielding;
        else if (Contains(GetRect(PanelType.PlayerIcon), pos))      return PanelType.PlayerIcon;
        else if (Contains(GetRect(PanelType.Character), pos))       return PanelType.Character;
        else if (Contains(GetRect(PanelType.Info), pos))            return PanelType.Info;
        else if (Contains(GetRect(PanelType.Chatbox), pos))         return PanelType.Chatbox;

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
			case PanelType.ArenaGrid:       return MainPanel.ArenaPanel;
			case PanelType.InventoryGrid:   return MainPanel.InventoryPanel;
			case PanelType.Log:             return MainPanel.LogPanel;
			case PanelType.Nearby:          return MainPanel.NearbyPanel;
			case PanelType.Character:       return MainPanel.CharacterPanel;
            case PanelType.EquipmentGrid:   return MainPanel.CharacterPanel.EquipmentPanel;
            case PanelType.Wielding:        return MainPanel.CharacterPanel.WieldingPanel;
            case PanelType.PlayerIcon:      return MainPanel.CharacterPanel.PlayerIcon;
            case PanelType.Info:            return MainPanel.InfoPanel;
            case PanelType.Chatbox:         return MainPanel.Chatbox;
            case PanelType.LevelLabel:      return MainPanel.ArenaPanel.LevelLabel;
        }

		return null;
	}

    public GridPanel GetGridPanel(GridType gridType)
    {
        Game.AssertClient();

        switch (gridType)
        {
            case GridType.Arena:        return MainPanel?.ArenaPanel ?? null;
            case GridType.Inventory:    return MainPanel?.InventoryPanel ?? null;
            case GridType.Equipment:    return MainPanel.CharacterPanel?.EquipmentPanel ?? null;
        }

		return null;
    }

	public GridType GetGridType(PanelType panelType)
    {
		switch(panelType)
		{
            case PanelType.ArenaGrid:       return GridType.Arena;
            case PanelType.InventoryGrid:   return GridType.Inventory;
            case PanelType.EquipmentGrid:   return GridType.Equipment;
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

    public string GetUnusableClass(Thing thing)
    {
        var gridManager = thing.ContainingGridManager;
        if (thing.Flags.HasFlag(ThingFlags.Useable) && gridManager.GridType == GridType.Inventory)
        {
            var owningPlayer = gridManager.OwningPlayer;
            if (owningPlayer != null && !thing.CanBeUsedBy(owningPlayer, ignoreResources: true))
                return "unusable_item";
        }

        return "";
    }

    public string GetEquipmentHighlightClass(Thing thing)
    {
        if(GetContainingPanelType(MousePosition) == PanelType.EquipmentGrid && !IsDraggingThing)
        {
            var gridManager = thing.ContainingGridManager;
            if (thing.Flags.HasFlag(ThingFlags.Equipment) && gridManager.GridType == GridType.Inventory)
            {
                return "equipment_item_highlight";
            }
        }

        return "";
    }

    public void AddFloater(string icon, IntVector gridPos, float time, Vector2 offsetStart, Vector2 offsetEnd, string text = "", bool requireSight = true, EasingType offsetEasingType = EasingType.Linear, float fadeInTime = 0f, float scale = 1f, Thing parent = null)
    {
        Floaters.Add(new FloaterData(icon, gridPos, time, offsetStart, offsetEnd, text, requireSight, offsetEasingType, fadeInTime, scale, parent));
    }

    public void RemoveFloater(string icon, Thing parent = null)
    {
        for(int i = Floaters.Count - 1; i >= 0; i--)
        {
            var floater = Floaters[i];
            if (floater.parent == parent && (floater.icon == icon || string.IsNullOrEmpty(icon)))
            {
                Floaters.RemoveAt(i);
            }
        }
    }

    public void Restart()
    {
        Floaters.Clear();
        StopDragging();
        MainPanel.Chatbox.Restart();
    }
}
