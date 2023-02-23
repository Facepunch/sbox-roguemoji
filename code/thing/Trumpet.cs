using Sandbox;
using System;

namespace Roguemoji;
public partial class Trumpet : Thing
{
    [Net] public int EnergyCost { get; private set; }
    public float CooldownTime { get; private set; }

    public override string AbilityName => "Blow Trumpet";

    public Trumpet()
	{
		DisplayIcon = "🎺";
        DisplayName = "Trumpet";
        Description = "Loud and annoying";
        Tooltip = "A trumpet";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;
        Flammability = 6;

        if (Game.IsServer)
        {
            EnergyCost = 1;
            CooldownTime = 1f;

            InitStat(StatType.Attack, 1);

            AddTrait(AbilityName, "🎺", $"Make a very loud noise", offset: new Vector2(0f, -1f), tattooIcon: "🎵", tattooScale: 0.42f, tattooOffset: new Vector2(12f, -12f), isAbility: true);
            AddTrait("", GetStatIcon(StatType.Energy), $"Ability costs {EnergyCost}{GetStatIcon(StatType.Energy)}", offset: new Vector2(0f, -3f), labelText: $"{EnergyCost}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "⏳", $"Cooldown time: {CooldownTime}s", offset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        var energy = user.GetStatClamped(StatType.Energy);
        if (energy < EnergyCost && !ignoreResources)
        {
            if (shouldLogMessage && user.Brain is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {EnergyCost}{GetStatIcon(StatType.Energy)} to use {ChatDisplayIcons} but you only have {energy}{GetStatIcon(StatType.Energy)}");

            return false;
        }

        return true;
    }

    public override void Use(Thing user)
    {
        if (IsOnCooldown)
            return;

        if (!user.TrySpendStat(StatType.Energy, EnergyCost))
            return;

        var startOffset = user.WieldedThingOffset + new Vector2(13f, 8f);
        var endOffset = startOffset + new Vector2(Game.Random.Float(20f, 40f), Game.Random.Float(-10f, -50f));
        var icon = Game.Random.Int(0, 10) == 0 ? "🎶" : "🎵";
        var time = Game.Random.Float(0.7f, 0.9f);
        var scale = Game.Random.Float(0.4f, 0.55f);
        var shakeAmount = Game.Random.Float(0f, 0.8f);
        user.AddFloater(icon, time, startOffset, endOffset, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.025f, scale: scale, opacity: 1f, shakeAmount: shakeAmount);

        StartCooldown(CooldownTime);

        base.Use(user);
    }

    public override void GetSound(SoundActionType actionType, SurfaceType surfaceType, out string sfxName, out int loudness)
    {
        switch (actionType)
        {
            case SoundActionType.Use:
                sfxName = "trumpet";
                loudness = 9;
                return;
            case SoundActionType.HitOther:
                sfxName = "trumpet";
                loudness = 9;
                return;
        }

        base.GetSound(actionType, surfaceType, out sfxName, out loudness);
    }
}
