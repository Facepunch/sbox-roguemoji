using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class BookTeleport : Thing
{
    public int ManaCost { get; private set; }
    public int ReqInt { get; private set; }
    public float CooldownTime { get; private set; }

    public override string ChatDisplayIcons => "📘🧭";
    public override string AbilityName => "Read Book";

    public BookTeleport()
	{
		DisplayIcon = "📘";
        DisplayName = "Book of Teleport";
        Description = "Teleport to a random place on the current floor.";
        Tooltip = "A book of Teleport.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
        Flags = ThingFlags.Selectable | ThingFlags.Useable;

        SetTattoo("🧭", scale: 0.5f, offset: new Vector2(0.5f, -2f), offsetWielded: new Vector2(0f, -2f), offsetInfo: new Vector2(1f, -1f), offsetCharWielded: new Vector2(2f, -4f), offsetInfoWielded: new Vector2(-1f, -2f));

        if (Game.IsServer)
        {
            ManaCost = 2;
            ReqInt = 6;
            CooldownTime = 5f;

            AddTrait(AbilityName, "📖", "Spend 🔮 to cast the spell Blink.", offset: new Vector2(0f, -2f), tattooIcon: "🧭", tattooScale: 0.7f, tattooOffset: new Vector2(0f, -5f));
            AddTrait("", "🔮", $"{ManaCost}🔮 used to cast spell.", offset: new Vector2(0f, -1f), labelText: $"{ManaCost}", labelFontSize: 15, labelOffset: new Vector2(0.5f, -1f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "🧠", $"{ReqInt}🧠 required to read.", offset: new Vector2(0f, -1f), labelText: $"≥{ReqInt}", labelFontSize: 15, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
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

    public override void Use(Thing user)
    {
        if (!user.TrySpendMana(ManaCost))
            return;

        if (user.ContainingGridManager.GetRandomEmptyGridPos(out var targetGridPos, allowNonSolid: true))
        {
            RoguemojiGame.Instance.AddFloater("✨", user.GridPos, 0.8f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), "", requireSight: true, EasingType.SineOut, fadeInTime: 0.2f);
            RoguemojiGame.Instance.AddFloater("✨", targetGridPos, 0.5f, user.CurrentLevelId, new Vector2(0, -3f), new Vector2(0, -4f), "", requireSight: true, EasingType.SineOut, fadeInTime: 0.1f);

            user.SetGridPos(targetGridPos);

            if (user is RoguemojiPlayer player)
                player.RecenterCamera();

            StartCooldown(CooldownTime);

            base.Use(user);
        }
    }
}
