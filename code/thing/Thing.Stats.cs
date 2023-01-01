using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum StatType { Health, Energy, Mana, Attack, Strength, Speed, Intelligence, Charisma, Sight, Hearing, Smell }

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
    }
}

public partial class Thing : Entity
{
	[Net] public bool HasStats { get; private set; }
	[Net] public IDictionary<StatType, Stat> Stats { get; private set; }

    [Net] public IDictionary<StatType, int> StatsCurrent { get; private set; }
    [Net] public IDictionary<StatType, int> StatsMin { get; private set; }
    [Net] public IDictionary<StatType, int> StatsMax { get; private set; }

    public static string GetStatIcon(StatType statType)
    {
        switch(statType)
        {
            case StatType.Health: return "❤️";
            case StatType.Energy: return "🔋";
            case StatType.Mana: return "🔮";
            case StatType.Attack: return "⚔️";
            case StatType.Strength: return "💪";
            case StatType.Speed: return "🕓️";
            case StatType.Intelligence: return "🧠";
            case StatType.Charisma: return "💋";
            case StatType.Sight: return "👁";
            case StatType.Hearing: return "👂️";
            case StatType.Smell: return "👃";
        }

        return "❓️";
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
            case StatType.Charisma: return "Charisma";
            case StatType.Sight: return "Sight";
            case StatType.Hearing: return "Hearing";
            case StatType.Smell: return "Smell";
        }

        return "???";
    }

    public static string GetStatDescription(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return "Amount of life remaining.";
            case StatType.Energy: return "Resource used for abilities.";
            case StatType.Mana: return "Magical resource used for spells.";
            case StatType.Attack: return "Amount of physical damage dealt.";
            case StatType.Strength: return "Physical power and ability to move heavy objects.";
            case StatType.Speed: return "Reduces the delay between actions.";
            case StatType.Intelligence: return "Skill with magic and technology.";
            case StatType.Charisma: return "Likeability and attractiveness.";
            case StatType.Sight: return "The ability to see farther and see past objects.";
            case StatType.Hearing: return "The ability to notice sounds from a distance.";
            case StatType.Smell: return "The ability to detect odors left by things.";
        }

        return "???";
    }

    public static string GetStatColor(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health: return "#ff1111";
            case StatType.Energy: return "#33ff33";
            case StatType.Mana: return "#8888ff";
            case StatType.Attack: return "#444444";
            case StatType.Strength: return "#ff8844";
            case StatType.Speed: return "#5555ff";
            case StatType.Intelligence: return "#9922ff";
            case StatType.Charisma: return "#ffff55";
            case StatType.Sight: return "#448844";
            case StatType.Hearing: return "#aa5500";
            case StatType.Smell: return "#5b3e31";
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
        }

        return false;
    }

    public static bool IsHiddenOnInfoPanel(StatType statType)
    {
        switch (statType)
        {
            
        }

        return false;
    }

    public virtual void InitStat(StatType statType, int current, int min = 0, int max = int.MaxValue, bool isModifier = false)
	{
		if (!HasStats)
		{
            if(Stats == null)
			    Stats = new Dictionary<StatType, Stat>();

			HasStats = true;
		}

        Stats[statType] = new Stat()
        {
            StatType = statType,
            CurrentValue = current,
            MinValue = min,
            MaxValue = max,
            IsModifier = isModifier,
		};
    }

    public void AdjustStat(StatType statType, int amount)
	{
		if (HasStats && Stats.ContainsKey(statType))
		{
            var stat = Stats[statType];
            var oldValue = stat.CurrentValue;

            stat.CurrentValue += amount;

            if (ShouldClampCurrentStat(statType))
                stat.CurrentValue = stat.ClampedValue;

            if(stat.CurrentValue != oldValue)
                OnChangedStat(statType);
        }
    }

    public void AdjustStatMin(StatType statType, int amount)
    {
        if (HasStats && Stats.ContainsKey(statType))
		{
            Stats[statType].MinValue += amount;
            OnChangedStat(statType);
        }
    }

    public void AdjustStatMax(StatType statType, int amount)
    {
        if (HasStats && Stats.ContainsKey(statType))
		{
            Stats[statType].MaxValue += amount;
			OnChangedStat(statType);
        }
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
        Stats.Clear();
        HasStats = false;
    }

    /// <summary> Clamp non-permanent stats like Health, don't clamp stats like Sight. </summary>
    public bool ShouldClampCurrentStat(StatType statType)
    {
        if (statType == StatType.Health)
            return true;

        return false;
    }
}
