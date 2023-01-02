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

    public override string ChatDisplayIcons => "📘✨";
    public override string AbilityName => "Read Book";

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

            AddTrait(AbilityName, "📖", "Spend 🔮 to cast the spell Blink.", offset: new Vector2(0f, -2f), tattooIcon: "✨", tattooScale: 0.7f, tattooOffset: new Vector2(0f, -5f));
            AddTrait("", "🔮", $"{ManaCost}🔮 required to cast spell.", offset: new Vector2(0f, -1f), labelText: $"{ManaCost}", labelFontSize: 15, labelOffset: new Vector2(0.5f, -1f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "🧠", $"{ReqInt}🧠 required to read book.", offset: new Vector2(0f, -1f), labelText: $"≥{ReqInt}", labelFontSize: 15, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "⏳", $"Cooldown time: {CooldownTime}s", offset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 15, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool TryStartUsing(Thing user)
    {
        var intelligence = user.GetStatClamped(StatType.Intelligence);
        if (intelligence < ReqInt)
        {
            if (user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {ReqInt}🧠 to use {ChatDisplayIcons} but you only have {intelligence}🧠");

            return false;
        }

        var mana = user.GetStatClamped(StatType.Mana);
        if(mana < ManaCost)
        {
            if(user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {ManaCost}🔮 to use {ChatDisplayIcons} but you only have {mana}🔮");

            return false;
        }

        return true;
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

        return ScrollBlink.BlinkGetAimingCells(Radius, ThingWieldingThis);
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        return ScrollBlink.BlinkIsPotentialAimingCell(gridPos, Radius, ThingWieldingThis);
    }
}
