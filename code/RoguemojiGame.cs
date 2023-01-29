using System;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public class PanelFlickerData
{
    public Panel panel;
    public int numFrames;

    public PanelFlickerData(Panel _panel)
    {
        panel = _panel;
        numFrames = 0;
    }
}

public enum LevelId
{
	None,
	Forest0, Forest1, Forest2, Forest3,
}

public partial class RoguemojiGame : GameManager
{
	public static RoguemojiGame Instance { get; private set; }

	public static int PlayerNum { get; set; }
	public static uint ThingId { get; set; }

	public Hud Hud { get; private set; }

    public const int CellSize = 42;

	public const int ArenaPanelWidth = 25;
	public const int ArenaPanelHeight = 19;

    // todo: move to player
    public const int InventoryWidth = 12; //5;
	public const int InventoryHeight = 6;
    public const int EquipmentWidth = 4;
    public const int EquipmentHeight = 2;

    public int LevelWidth { get; set; }
	public int LevelHeight { get; set; }

	public record struct LogData(string text, int playerNum);
	public Queue<LogData> LogMessageQueue = new Queue<LogData>();
    public Queue<LogData> ChatMessageQueue = new Queue<LogData>();

    public List<RoguemojiPlayer> Players = new List<RoguemojiPlayer>();

	public RoguemojiPlayer LocalPlayer => Game.LocalPawn as RoguemojiPlayer; // Client-only

	public List<PanelFlickerData> _panelsToFlicker;

    [Net] public IDictionary<LevelId, Level> Levels { get; private set; }

    [Net] public IList<string> UnidentifiedScrollSymbols { get; private set; }
    [Net] public IList<string> UnidentifiedScrollNames { get; private set; }
    [Net] public IList<string> UnidentifiedPotionSymbols{ get; private set; }
    [Net] public IList<string> UnidentifiedPotionNames { get; private set; }

    public RoguemojiGame()
	{
		Instance = this;

		if (Game.IsServer)
		{
            Levels = new Dictionary<LevelId, Level>();
            CreateLevel(LevelId.Forest0);

            UnidentifiedScrollSymbols = new List<string>() { "🈁", "🈂️", "🈷️", "🈯️", "🈹", "🈳", "🈚️", "🈸", "🈴", }; // 🈶 🈺 🈵
            UnidentifiedScrollSymbols.Shuffle();
            UnidentifiedScrollNames = new List<string>() { "WYZ'LOK", "MYR'KLYN", "PHYZGRYF", "XORPHYX", "GRYFAD", "RYXORK", "ORAXUM", "ZORKOZAL", "KLYNX", "QYN", "ARPHYNY", "LUZ'ROKLUM", "YNDRYNY", "PYG'JYG", "BRAX'PHY", "FEN'XOR", "CIRXYX" };
            UnidentifiedScrollNames.Shuffle();

            UnidentifiedPotionSymbols = new List<string>() { "🉑", "🔘", "🧿", "🌐", "🌓", "🌑", "🌕️", "🌙", "©️", "®️", "㊗️", "㊙️", "⚫️", "🟤", "⭕️", "Ⓜ️", "🍥", "🟢", "🟣", }; // 🉐🌒🌔🌖🌘🌗
            UnidentifiedPotionSymbols.Shuffle();
            UnidentifiedPotionNames = new List<string>() { "cloudy", "misty", "murky", "sparkling", "fizzy", "bubbly", "smoky", "congealed", "chalky", "radiant", "milky", "thick", "pasty", "glossy", "dull", "dusty", "syrupy", "pungent", 
                "viscous", "sludgy", "pale", "filmy", "rusty", "chunky", "creamy", "hazy", "silky", "foggy", "pulpy", "dark", "oily", "opaque", "shiny", "frothy", "wavy" };
            UnidentifiedPotionNames.Shuffle();
        }

		if (Game.IsClient)
		{
			Hud = new Hud();
			_panelsToFlicker = new List<PanelFlickerData>();
        }
	}

    public string GetUnidentifiedScrollIcon(ScrollType scrollType) { return UnidentifiedScrollSymbols[(int)scrollType]; }
    public string GetUnidentifiedScrollName(ScrollType scrollType) { return UnidentifiedScrollNames[(int)scrollType]; }
    public string GetUnidentifiedPotionIcon(PotionType potionType) { return UnidentifiedPotionSymbols[(int)potionType]; }
    public string GetUnidentifiedPotionName(PotionType potionType) { return UnidentifiedPotionNames[(int)potionType]; }

    HashSet<LevelId> _occupiedLevelIds = new HashSet<LevelId>();

    [Event.Tick.Server]
	public void ServerTick()
	{
		float dt = Time.Delta;

        _occupiedLevelIds.Clear();
        foreach (RoguemojiPlayer player in Players)
		{
			if(player != null && player.IsValid)
                _occupiedLevelIds.Add(player.CurrentLevelId);
		}

		foreach(var levelId in _occupiedLevelIds)
		{
			Level level = Levels[levelId];
			level.Update(dt);
		}
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		if (Hud.MainPanel.LogPanel != null)
		{
			while (LogMessageQueue.Count > 0)
			{
				var data = LogMessageQueue.Dequeue();
				Hud.MainPanel.LogPanel.WriteMessage(data.text, data.playerNum);
			}
		}

        if (Hud.MainPanel.ChatPanel != null)
        {
            while (ChatMessageQueue.Count > 0)
            {
                var data = ChatMessageQueue.Dequeue();
                Hud.MainPanel.ChatPanel.WriteMessage(data.text, data.playerNum);
            }
        }

        for (int i = _panelsToFlicker.Count - 1; i >= 0; i--)
        {
            var data = _panelsToFlicker[i];
            data.numFrames++;

            if (data.numFrames >= 2)
            {
                if (data.panel != null)
                    data.panel.Style.PointerEvents = PointerEvents.All;

                _panelsToFlicker.RemoveAt(i);
            }
        }
    }

	public override void ClientJoined(IClient client) // Server-only
	{
		base.ClientJoined(client);

		var level0 = Levels[LevelId.Forest0];

        level0.GridManager.GetRandomEmptyGridPos(out var gridPos);
        RoguemojiPlayer player = level0.GridManager.SpawnThing<RoguemojiPlayer>(gridPos);
		player.CurrentLevelId = LevelId.Forest0;
		player.PlayerNum = ++PlayerNum;
		player.RecenterCamera();

		client.Pawn = player;
        player.RefreshVisibility(To.Single(player));

		Players.Add(player);
    }

	public override void ClientDisconnect(IClient client, NetworkDisconnectionReason reason)
	{
		var player = client.Pawn as RoguemojiPlayer;

		var level = Levels[player.CurrentLevelId];
		level.GridManager.RemoveThing(player);

		// todo: drop or remove items in player's inventory

		Players.Remove(player);

		base.ClientDisconnect(client, reason);
	}

    public void LogMessage(string text, int playerNum)
	{
		LogMessageClient(text, playerNum);
	}

    public void LogPersonalMessage(RoguemojiPlayer player, string text)
    {
        LogMessageClient(To.Single(player), text, playerNum: 0);
    }

    [ClientRpc]
	public void LogMessageClient(string text, int playerNum)
	{
		if (Hud.MainPanel.LogPanel == null)
		{
			LogMessageQueue.Enqueue(new LogData(text, playerNum));
			return;
		}

		Hud.MainPanel.LogPanel.WriteMessage(text, playerNum);
	}

    public void ChatMessage(string text, int playerNum)
    {
        ChatMessageClient(text, playerNum);
    }

    [ClientRpc]
    public void ChatMessageClient(string text, int playerNum)
    {
        if (Hud.MainPanel.ChatPanel == null)
        {
            ChatMessageQueue.Enqueue(new LogData(text, playerNum));
            return;
        }

        Hud.MainPanel.ChatPanel.WriteMessage(text, playerNum);
    }

    [ConCmd.Server]
	public static void GridCellClickedCmd(int x, int y, GridType gridType, bool rightClick, bool shift, bool doubleClick, bool visible = true)
	{
		var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
		player.GridCellClicked(new IntVector(x, y), gridType, rightClick, shift, doubleClick, visible);
	}

    [ConCmd.Server]
    public static void ConfirmAimingCmd(GridType gridType, int x, int y)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        player.ConfirmAiming(gridType, new IntVector(x, y));
    }

    [ConCmd.Server]
    public static void StopAimingCmd()
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        player.StopAiming();
    }

    [ClientRpc]
	public void RefreshGridPanelClient(GridType gridType)
	{
		GridPanel panel = Hud.Instance.GetGridPanel(gridType);

		if(panel != null)
			panel.StateHasChanged();
	}

    [ConCmd.Server]
    public static void NearbyThingClickedCmd(int networkIdent, bool rightClick, bool shift, bool doubleClick)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;

		if (thing.ContainingGridType != GridType.Arena)
		{
			Log.Info("Trying to pick up " + thing.Name + " but it's no longer on the ground!");
            return;
        }

        player.NearbyThingClicked(thing, rightClick, shift, doubleClick);
    }

    [ClientRpc]
    public void RefreshNearbyPanelClient()
    {
		Hud.Instance.MainPanel.NearbyPanel?.StateHasChanged();
    }

    [ClientRpc]
    public void FlickerNearbyPanelCellsClient()
    {
		var nearbyPanel = Hud.Instance.MainPanel?.NearbyPanel;
        if (nearbyPanel == null)
			return;

        nearbyPanel.FlickerCells();
    }

    [ClientRpc]
	public void FlickerWieldingPanel()
	{
		var wieldingPanel = Hud.Instance.MainPanel.CharacterPanel.WieldingPanel;
		FlickerPanel(wieldingPanel);
    }

    public void FlickerPanel(Panel panel)
	{
		Game.AssertClient();

		if (panel == null)
			return;

		panel.Style.PointerEvents = PointerEvents.None;
		_panelsToFlicker.Add(new PanelFlickerData(panel));
	}

    [ConCmd.Server]
    public static void InventoryThingDraggedCmd(int networkIdent, PanelType destinationPanelType, int x, int y, bool wieldedThingDragged)
	{
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
		player.InventoryThingDragged(thing, destinationPanelType, new IntVector(x, y), wieldedThingDragged);
    }

    [ConCmd.Server]
    public static void EquipmentThingDraggedCmd(int networkIdent, PanelType destinationPanelType, int x, int y)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
        player.EquipmentThingDragged(thing, destinationPanelType, new IntVector(x, y));
    }

    [ConCmd.Server]
    public static void NearbyThingDraggedCmd(int networkIdent, PanelType destinationPanelType, int x, int y)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        Thing thing = FindByIndex(networkIdent) as Thing;
        player.NearbyThingDragged(thing, destinationPanelType, new IntVector(x, y));
    }

    [ConCmd.Server]
    public static void WieldingClickedCmd(bool rightClick, bool shift)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        player.WieldingClicked(rightClick, shift);
    }

    [ConCmd.Server]
    public static void PlayerIconClickedCmd(bool rightClick, bool shift)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        player.PlayerIconClicked(rightClick, shift);
    }

    public RoguemojiPlayer GetClosestPlayer(IntVector gridPos)
	{
		int closestDistance = int.MaxValue;
		RoguemojiPlayer closestPlayer = null;

		foreach (var player in Players) 
		{
			if (!player.IsValid)
				continue;

			int dist = (player.GridPos - gridPos).ManhattanLength;
			if(dist < closestDistance )
			{
				closestDistance = dist;
				closestPlayer = player;
			}
		}

		return closestPlayer;
	}

	public void Restart()
	{
        UnidentifiedScrollSymbols.Shuffle();
        UnidentifiedScrollNames.Shuffle();
        UnidentifiedPotionSymbols.Shuffle();
        UnidentifiedPotionNames.Shuffle();

        foreach (var pair in Levels)
			pair.Value.Restart();

        foreach (RoguemojiPlayer player in Players)
		{
			player.Restart();

			ChangeThingLevel(player, LevelId.Forest0);
		}

        Log.Info($"# Entities: {Entity.All.Count()}");
    }

    [ClientRpc]
	public void ResetHudClient()
	{
		Hud.Restart();
	}

	public void ChangeThingLevel(Thing thing, LevelId levelId, bool shouldAnimateFall = false)
	{
        if (thing.CurrentLevelId != LevelId.None)
        {
            var oldLevel = Levels[thing.CurrentLevelId];
            oldLevel.GridManager.RemoveThing(thing);
        }

        var level = Levels.ContainsKey(levelId) ? Levels[levelId] : CreateLevel(levelId);
        var gridManager = level.GridManager;

        gridManager.AddThing(thing);
        thing.CurrentLevelId = levelId;

        gridManager.GetRandomEmptyGridPos(out var gridPos);
        thing.SetGridPos(gridPos);

        if(thing is RoguemojiPlayer player)
        {
            player.RecenterCamera();
            ResetHudClient(To.Single(player));
        }
    }

	Level CreateLevel(LevelId levelId)
	{
        var level = new Level();
        level.Init(levelId);
        Levels.Add(levelId, level);

		return level;
    }

	public Level GetLevel(LevelId levelId)
	{
		if (Levels.ContainsKey(levelId))
			return Levels[levelId];

		return null;
	}

    public T SpawnThing<T>(LevelId levelId) where T : Thing
    {
        Game.AssertServer();

        var thing = TypeLibrary.GetType(typeof(T)).Create<T>();
        thing.CurrentLevelId = levelId;

        thing.OnSpawned();

        return thing;
    }

    public void AddFloaterInventory(RoguemojiPlayer player, string icon, IntVector gridPos, float time, Vector2 offsetStart, Vector2 offsetEnd, float height = 0f, string text = "", 
        EasingType offsetEasingType = EasingType.Linear, float fadeInTime = 0f, float scale = 1f, float opacity = 1f, Thing parent = null)
    {
        AddFloaterClient(To.Single(player), icon, gridPos.x, gridPos.y, time, offsetStart, offsetEnd, height, text, requireSight: false, alwaysShowWhenAdjacent: false, offsetEasingType, fadeInTime, scale, opacity, GridType.Inventory, (parent != null) ? parent.NetworkIdent : -1);
    }

    public void AddFloater(string icon, IntVector gridPos, float time, LevelId levelId, Vector2 offsetStart, Vector2 offsetEnd, float height = 0f, string text = "", bool requireSight = true, bool alwaysShowWhenAdjacent = false, 
                        EasingType offsetEasingType = EasingType.Linear, float fadeInTime = 0f, float scale = 1f, float opacity = 1f, Thing parent = null)
    {
		foreach(var player in Players)
		{
			if (player.CurrentLevelId == levelId)
            {
                AddFloaterClient(To.Single(player), icon, gridPos.x, gridPos.y, time, offsetStart, offsetEnd, height, text, requireSight, alwaysShowWhenAdjacent, offsetEasingType, fadeInTime, scale, opacity, GridType.Arena, (parent != null) ? parent.NetworkIdent : -1);
            }
		}
    }

    [ClientRpc]
    public void AddFloaterClient(string icon, int x, int y, float time, Vector2 offsetStart, Vector2 offsetEnd, float height = 0f, string text = "", bool requireSight = true, bool alwaysShowWhenAdjacent = false, 
                                EasingType offsetEasingType = EasingType.Linear, float fadeInTime = 0f, float scale = 1f, float opacity = 1f, GridType gridType = GridType.Arena, int parentIdent = -1)
    {
        Thing parent = (parentIdent == -1) ? null : Entity.FindByIndex(parentIdent) as Thing;
        Hud.Instance.AddFloater(icon, new IntVector(x, y), time, offsetStart, offsetEnd, height, text, requireSight, alwaysShowWhenAdjacent, offsetEasingType, fadeInTime, scale, opacity, gridType, parent);
    }

    public void RemoveFloater(string icon, LevelId levelId, Thing parent = null)
    {
        foreach (var player in Players)
        {
            if (player.CurrentLevelId == levelId)
                RemoveFloaterClient(To.Single(player), icon, (parent != null) ? parent.NetworkIdent : -1);
        }
    }

    [ClientRpc]
    public void RemoveFloaterClient(string icon, int parentIdent = -1)
    {
        Thing parent = (parentIdent == -1) ? null : Entity.FindByIndex(parentIdent) as Thing;
        Hud.Instance.RemoveFloater(icon, parent);
    }

    public void RemoveFloaters(LevelId levelId, Thing parent = null)
    {
        foreach (var player in Players)
        {
            if (player.CurrentLevelId == levelId)
                RemoveFloatersClient(To.Single(player), (parent != null) ? parent.NetworkIdent : -1);
        }
    }

    [ClientRpc]
    public void RemoveFloatersClient(int parentIdent = -1)
    {
        Thing parent = (parentIdent == -1) ? null : Entity.FindByIndex(parentIdent) as Thing;

        if(parent != null)
            Hud.Instance.RemoveFloaters(parent);
    }

    public void RevealScroll(ScrollType scrollType, IntVector gridPos, LevelId levelId)
    {
        foreach (var player in Players)
        {
            if (player.CurrentLevelId == levelId)
                RevealScrollClient(To.Single(player), scrollType, gridPos);
        }
    }

    [ClientRpc]
    public void RevealScrollClient(ScrollType scrollType, IntVector gridPos)
    {
        var player = LocalPlayer;
        if (player.IsCellVisible(gridPos) && !player.IsScrollTypeIdentified(scrollType))
        {
            RevealScrollCmd(scrollType);
        }
    }

    [ConCmd.Server]
    public static void RevealScrollCmd(ScrollType scrollType)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        player.IdentifyScroll(scrollType);
    }

    public void RevealPotion(PotionType potionType, IntVector gridPos, LevelId levelId)
    {
        foreach (var player in Players)
        {
            if (player.CurrentLevelId == levelId)
                RevealPotionClient(To.Single(player), potionType, gridPos);
        }
    }

    [ClientRpc]
    public void RevealPotionClient(PotionType potionType, IntVector gridPos)
    {
        var player = LocalPlayer;
        if(player.IsCellVisible(gridPos) && !player.IsPotionTypeIdentified(potionType))
        {
            RevealPotionCmd(potionType);
        }
    }

    [ConCmd.Server]
    public static void RevealPotionCmd(PotionType potionType)
    {
        var player = ConsoleSystem.Caller.Pawn as RoguemojiPlayer;
        player.IdentifyPotion(potionType);
    }

    public void DebugGridLine(IntVector a, IntVector b, Color color, float time, GridType gridTypeA = GridType.Arena, GridType gridTypeB = GridType.Arena)
	{
        DebugGridLineClient(a, b, color, time, gridTypeA, gridTypeB);
    }

    [ClientRpc]
	public void DebugGridLineClient(IntVector a, IntVector b, Color color, float time, GridType gridTypeA = GridType.Arena, GridType gridTypeB = GridType.Arena)
	{
		Hud.Instance.DebugDrawing.GridLine(a, b, color, time, gridTypeA, gridTypeB);
	}

    public void DebugGridCell(IntVector gridPos, Color color, float time, GridType gridType = GridType.Arena)
    {
        DebugGridCellClient(gridPos, color, time, gridType);
    }

    [ClientRpc]
    public void DebugGridCellClient(IntVector gridPos, Color color, float time, GridType gridType)
    {
        Hud.Instance.DebugDrawing.GridCell(gridPos, color, time, gridType);
    }
}
