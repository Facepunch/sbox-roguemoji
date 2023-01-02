using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class BookBlink : Thing
{
    [Net] public int Radius { get; set; }

    public int ManaCost { get; private set; }
    public int ReqInt { get; private set; }
    public float CooldownTime { get; private set; }
    
    public BookBlink()
	{
		DisplayIcon = "📘";
        DisplayName = "Book of Blink";
        Description = "Teleport to a target location nearby.";
        Tooltip = "A book of Blink.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
        Flags = ThingFlags.Selectable | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        SetTattoo("✨", scale: 0.5f, offset: new Vector2(0.5f, -2f), offsetWielded: new Vector2(0f, -2f), offsetInfo: new Vector2(1f, -1f), offsetCharWielded: new Vector2(2f, -4f), offsetInfoWielded: new Vector2(-1f, -2f));

        if (Game.IsServer)
        {
            ManaCost = 2;
            ReqInt = 5;
            CooldownTime = 3f;
            AddTrait("", "🔮", $"Mana cost: {ManaCost}", offset: new Vector2(0f, -1f), labelText: $"{ManaCost}", labelFontSize: 15, labelOffset: new Vector2(0.5f, -1f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "🧠", $"Intelligence required: {ReqInt}", offset: new Vector2(0f, -1f), labelText: $"≥{ReqInt}", labelFontSize: 15, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "⏳", $"Cooldown time: {CooldownTime}s", offset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 15, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override void Use(Thing user, IntVector targetGridPos)
    {
        base.Use(user, targetGridPos);

        var things = ContainingGridManager.GetThingsAt(targetGridPos).WithAll(ThingFlags.Solid).ToList();
        if (things.Count > 0)
            return;

        RoguemojiGame.Instance.AddFloater("✨", user.GridPos, 0.8f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), "", requireSight: true, EasingType.SineOut, fadeInTime: 0.2f);
        RoguemojiGame.Instance.AddFloater("✨", targetGridPos, 0.5f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), "", requireSight: true, EasingType.SineOut, fadeInTime: 0.1f);

        user.SetGridPos(targetGridPos);

        StartCooldown(CooldownTime);
    }

    public override void OnWieldedBy(Thing thing)
    {
        base.OnWieldedBy(thing);

        Radius = Math.Max(thing.GetStatClamped(StatType.Intelligence), 1);
    }

    public override HashSet<IntVector> GetAimingTargetCellsClient() 
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        HashSet<IntVector> aimingCells = new HashSet<IntVector>();

        for(int x = -Radius; x <= Radius; x++)
        {
            for(int y = -Radius; y <= Radius; y++)
            {
                int distance = Utils.GetDistance(x, y);
                if (distance > Radius)
                    continue;

                var gridPos = ThingWieldingThis.GridPos + new IntVector(x, y);
                if (ThingWieldingThis.ContainingGridManager.GetThingsAtClient(gridPos).WithAll(ThingFlags.Solid).ToList().Count > 0)
                    continue;

                aimingCells.Add(gridPos);
            }
        }

        return aimingCells;
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        for (int x = -Radius; x <= Radius; x++)
        {
            for (int y = -Radius; y <= Radius; y++)
            {
                int distance = Utils.GetDistance(x, y);
                if (distance > Radius)
                    continue;

                var currGridPos = ThingWieldingThis.GridPos + new IntVector(x, y);
                if (gridPos.Equals(currGridPos))
                    return true;
            }
        }

        return false;
    }
}
