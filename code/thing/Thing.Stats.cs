using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum StatType { Health, Attack, Speed, Intelligence, Charisma, Sight, Hearing, Smell }

public partial class Stat : BaseNetworkable
{
    [Net] public StatType StatType { get; set; }
    [Net] public int CurrentValue { get; set; }
    [Net] public int MinValue { get; set; }
    [Net] public int MaxValue { get; set; }
}

public partial class Thing : Entity
{
	[Net] public bool HasStats { get; private set; }
	[Net] public IDictionary<StatType, Stat> Stats { get; private set; }

    public static string GetStatIcon(StatType statType)
    {
        switch(statType)
        {
            case StatType.Health: return "❤️";
            case StatType.Attack: return "⚔️";
            case StatType.Speed: return "⏳";
            case StatType.Intelligence: return "🧠";
            case StatType.Charisma: return "👄";
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
            case StatType.Attack: return "Attack";
            case StatType.Speed: return "Speed";
            case StatType.Intelligence: return "Intelligence";
            case StatType.Charisma: return "Charisma";
            case StatType.Sight: return "Sight";
            case StatType.Hearing: return "Hearing";
            case StatType.Smell: return "Smell";
        }

        return "???";
    }

    public virtual void InitStat(StatType statType, int current, int min, int max)
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
		};
    }

    public void AdjustStat(StatType statType, int amount)
	{
		if (HasStats && Stats.ContainsKey(statType))
		{
            Stats[statType].CurrentValue += amount;
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

	public virtual void OnChangedStat(StatType statType) 
    {
        
    }

    public bool HasStat(StatType statType)
    {
        return HasStats && Stats.ContainsKey(statType);
    }

    public int GetStat(StatType statType)
	{
		if (HasStats && Stats.ContainsKey(statType))
		{
			var stat = Stats[statType];
			return Math.Clamp(stat.CurrentValue, stat.MinValue, stat.MaxValue);
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
}
