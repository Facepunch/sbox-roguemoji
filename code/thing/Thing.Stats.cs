using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum StatType { Health, Strength, Speed, Intelligence, Charisma, Sight, Hearing, Smell }

public partial class Stat : BaseNetworkable
{
	[Net] public int CurrentValue { get; set; }
    [Net] public int MinValue { get; set; }
    [Net] public int MaxValue { get; set; }
    [Net] public string Icon { get; set; }
}

public partial class Thing : Entity
{
	[Net] public bool HasStats { get; private set; }
	[Net] public IDictionary<StatType, Stat> Stats { get; private set; }

	public virtual void InitStat(StatType statType, int current, int min, int max, string icon)
	{
		if (!HasStats)
		{
            if(Stats == null)
			    Stats = new Dictionary<StatType, Stat>();

			HasStats = true;
		}

        Stats[statType] = new Stat()
		{
			CurrentValue = current,
			MinValue = min,
			MaxValue = max,
            Icon = icon
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

    public string GetStatIcon(StatType statType)
    {
        if (HasStats && Stats.ContainsKey(statType))
            return Stats[statType].Icon;

        return "❓️";
    }

    public void ClearStats()
    {
        Stats.Clear();
        HasStats = false;
    }
}
