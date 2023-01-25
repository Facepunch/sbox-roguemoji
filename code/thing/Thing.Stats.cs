using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum StatType { 
    Health, Energy, Mana, Attack, Strength, Speed, Intelligence, Stamina, Stealth, Charisma, Sight, Hearing, Smell,
    Durability, MaxHealth, 
    Invisible, SeeInvisible, SightBlockAmount,
}

public partial class Stat : Entity
{
    [Net] public StatType StatType { get; set; }
    [Net] public int CurrentValue { get; set; }
    [Net] public int MinValue { get; set; }
    [Net] public int MaxValue { get; set; }
    [Net] public bool IsModifier { get; set; }

    public int ClampedValue => Math.Clamp(CurrentValue, MinValue, MaxValue);
    public int HashCode => CurrentValue + MinValue + MaxValue;

    public Stat()
    {
        Transmit = TransmitType.Always;
        CurrentValue = 0;
    }
}

public partial class Thing : Entity
{
	[Net] public bool HasStats { get; private set; }
	[Net] public IDictionary<StatType, Stat> Stats { get; private set; }

    [Net] public IDictionary<StatType, int> StatsCurrent { get; private set; }
    [Net] public IDictionary<StatType, int> StatsMin { get; private set; }
    [Net] public IDictionary<StatType, int> StatsMax { get; private set; }

    [Net] public int StatHash { get; private set; }

    public static string GetStatIcon(StatType statType)
    {
        switch(statType)
        {
            case StatType.Health: return "❤️";
            case StatType.Energy: return "🔋";
            case StatType.Mana: return "💠";
            case StatType.Attack: return "⚔️";
            case StatType.Strength: return "💪";
            case StatType.Speed: return "🏁";
            case StatType.Intelligence: return "🧠";
            case StatType.Stamina: return "🏃";
            case StatType.Stealth: return "👤";
            case StatType.Charisma: return "💋";
            case StatType.Sight: return "👁";
            case StatType.Hearing: return "👂️";
            case StatType.Smell: return "👃";

            case StatType.Durability: return "⚙️";
            case StatType.MaxHealth: return "💕";
        }

        return "";
    }

    public static string GetStatName(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return "Health";
            case StatType.Energy: return "Energy";
            case StatType.Mana: return "Mana";
            case StatType.Attack: return "Attack";
            case StatType.Strength: return "Strength";
            case StatType.Speed: return "Speed";
            case StatType.Intelligence: return "Intelligence";
            case StatType.Stamina: return "Stamina";
            case StatType.Stealth: return "Stealth";
            case StatType.Charisma: return "Charisma";
            case StatType.Sight: return "Sight";
            case StatType.Hearing: return "Hearing";
            case StatType.Smell: return "Smell";

            case StatType.Durability: return "Durability";
            case StatType.MaxHealth: return "Max Health";
        }

        return "";
    }

    public static string GetStatDescription(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return $"Amount of life remaining";
            case StatType.Energy: return $"Regenerating resource used for certain abilities";
            case StatType.Mana: return $"Magical resource used for spells";
            case StatType.Attack: return $"Amount of physical damage dealt";
            case StatType.Strength: return $"Physical power and ability to move heavy objects";
            case StatType.Speed: return $"Reduces the delay between actions";
            case StatType.Intelligence: return $"Skill with magic and technology";
            case StatType.Stamina: return $"Regenerates energy more quickly";
            case StatType.Stealth: return $"Skill at avoiding detection";
            case StatType.Charisma: return $"Likeability and attractiveness";
            case StatType.Sight: return $"The ability to see farther and see through objects";
            case StatType.Hearing: return $"The ability to notice sounds from a distance";
            case StatType.Smell: return $"The ability to detect odors left by things";

            case StatType.Durability: return $"Remaining physical integrity";
            case StatType.MaxHealth: return "Maximum amount of life available";
        }

        return "???";
    }

    public static string GetStatInfoDescription(StatType statType, Thing thing)
    {
        switch (statType)
        {
            case StatType.Speed: return $"Delay: {CActing.CalculateActionDelay(thing.GetStatClamped(StatType.Speed)).ToString("N2")}s";
            case StatType.Intelligence: return $"Increases {GetStatIcon(StatType.Mana)} capacity";
            case StatType.Stamina: return $"Increases {GetStatIcon(StatType.Energy)} capacity";
            case StatType.Stealth: return $"A value below 0 makes you more noticeable";
            case StatType.Charisma: return $"A value below zero makes you more disliked";
        }

        return "";
    }

    public static string GetStatColor(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return "#ff1111";
            case StatType.Energy: return "#33ff33";
            case StatType.Mana: return "#8888ff";
            case StatType.Attack: return "#aaaaaa";
            case StatType.Strength: return "#ff8844";
            case StatType.Speed: return "#5555ff";
            case StatType.Intelligence: return "#9922ff";
            case StatType.Stamina: return "#448844";
            case StatType.Stealth: return "#444444";
            case StatType.Charisma: return "#dd33bb";
            case StatType.Sight: return "#ffff55";
            case StatType.Hearing: return "#aa5500";
            case StatType.Smell: return "#5b3e31";

            case StatType.Durability: return "#aaaacc";
            case StatType.MaxHealth: return "#ff1111";
        }

        return "#ffffff";
    }

    public static bool ShouldShowOnCharacterPanel(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return true;
            case StatType.Energy: return true;
            case StatType.Mana: return true;
        }

        return false;
    }

    public static bool ShouldShowMaxOnTooltip(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return true;
            case StatType.Energy: return true;
            case StatType.Mana: return true;

            case StatType.Durability: return true;
        }

        return false;
    }

    public static bool ShouldShowBar(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return true;
            case StatType.Energy: return true;
            case StatType.Mana: return true;

            case StatType.Durability: return true;
        }

        return false;
    }

    public static bool IsHiddenOnInfoPanel(Stat stat)
    {
        switch (stat.StatType)
        {
            case StatType.Invisible: return true;
            case StatType.SeeInvisible: return true;
            case StatType.SightBlockAmount: return true;
            case StatType.Stealth: return (stat.ClampedValue == 0);
        }

        return false;
    }

    public virtual void InitStat(StatType statType, int current, int min = 0, int max = 999, bool isModifier = false)
	{
		if (!HasStats)
		{
            if(Stats == null)
			    Stats = new Dictionary<StatType, Stat>();

			HasStats = true;
		}

        Sandbox.Diagnostics.Assert.True(!Stats.ContainsKey(statType));

        Stats[statType] = new Stat()
        {
            StatType = statType,
            CurrentValue = current,
            MinValue = min,
            MaxValue = max,
            IsModifier = isModifier,
		};

        OnChangedStat(statType, changeCurrent: current, changeMin: min, changeMax: max);
    }

    public void AdjustStat(StatType statType, int amount)
	{
		if (HasStats && Stats.ContainsKey(statType))
		{
            var stat = Stats[statType];
            var oldValue = stat.CurrentValue;

            stat.CurrentValue += amount;

            if (ShouldClampCurrentValue(statType))
                stat.CurrentValue = stat.ClampedValue;

            int change = stat.CurrentValue - oldValue;
            if (change != 0)
                OnChangedStat(statType, changeCurrent: change, changeMin: 0, changeMax: 0);
        }
    }

    public void AdjustStatMin(StatType statType, int amount)
    {
        if (HasStats && Stats.ContainsKey(statType))
		{
            Stats[statType].MinValue += amount;
            OnChangedStat(statType, changeCurrent: 0, changeMin: amount, changeMax: 0);
        }
    }

    public void AdjustStatMax(StatType statType, int amount)
    {
        if (HasStats && Stats.ContainsKey(statType))
		{
            Stats[statType].MaxValue += amount;
            OnChangedStat(statType, changeCurrent: 0, changeMin: 0, changeMax: amount);
        }
    }

    public virtual bool TrySpendStat(StatType statType, int cost)
    {
        int available = GetStatClamped(statType);

        if (available < cost)
            return false;

        AdjustStat(statType, -cost);
        return true;
    }

    public bool HasStat(StatType statType)
    {
        return HasStats && Stats.ContainsKey(statType);
    }

    public Stat GetStat(StatType statType)
    {
        if (HasStats && Stats.ContainsKey(statType))
            return Stats[statType];

        return null;
    }

    public int GetStatClamped(StatType statType)
	{
		if (HasStats && Stats.ContainsKey(statType))
		{
			var stat = Stats[statType];
			return stat.ClampedValue;
        }

		return 0;
	}

    public int GetStatMin(StatType statType)
    {
        if (HasStats && Stats.ContainsKey(statType))
			return Stats[statType].MinValue;

        return 0;
    }

    public int GetStatMax(StatType statType)
    {
        if (HasStats && Stats.ContainsKey(statType))
            return Stats[statType].MaxValue;

        return 0;
    }

    public void ClearStats()
    {
        foreach (var pair in Stats)
            pair.Value.Delete();

        Stats.Clear();
        HasStats = false;
    }

    /// <summary> Clamp non-permanent resource stats like Health, don't clamp stats like Sight. </summary>
    public bool ShouldClampCurrentValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return true;
            case StatType.Energy: return true;
            case StatType.Mana: return true;
        }

        return false;
    }

    public bool ShouldShowInfoStats()
    {
        if (!HasStats)
            return false;

        foreach (var pair in Stats)
        {
            if (!IsHiddenOnInfoPanel(pair.Value))
                return true;
        }

        return false;
    }
}
