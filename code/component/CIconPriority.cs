using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public class IconPriorityData
{
    public string icon;
    public int priority;
    public float time;
    public float elapsedTime;

    public IconPriorityData(string icon, int priority, float time)
    {
        this.icon = icon;
        this.priority = priority;
        this.time = time;
        this.elapsedTime = 0f;
    }
}

public class CIconPriority : ThingComponent
{
    private string _defaultIcon = "";
    public Dictionary<int, IconPriorityData> IconPriorities = new Dictionary<int, IconPriorityData>();

    public int Counter { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        bool dirty = false;
        for (int i = IconPriorities.Count - 1; i >= 0; i--)
        {
            KeyValuePair<int, IconPriorityData> pair = IconPriorities.ElementAt(i);
            var data = pair.Value;

            if (data.time > 0f)
            {
                data.elapsedTime += dt;
                if (data.elapsedTime > data.time)
                {
                    var index = pair.Key;
                    IconPriorities.Remove(index);
                    dirty = true;
                }
            }
        }

        if (dirty)
            RefreshIcon();
    }

    /// <summary> Returns id number so you can override/remove it later. </summary>
    public int AddIconPriority(string icon, int priority = 0, float time = 0f)
    {
        int index = Counter++;
        IconPriorities.Add(index, new IconPriorityData(icon, priority, time));
        RefreshIcon();
        return index;
    }

    public bool SetIconPriority(int index, string icon, int priority = 0, float time = 0f)
    {
        if (IconPriorities.ContainsKey(index))
        {
            IconPriorities[index] = new IconPriorityData(icon, priority, time);
            RefreshIcon();
            return true;
        }

        return false;
    }

    public bool RemoveIconPriority(int index)
    {
        if(IconPriorities.ContainsKey(index))
        {
            IconPriorities.Remove(index);
            RefreshIcon();
            return true;
        }

        return false;
    }

    public void SetDefaultIcon(string icon)
    {
        _defaultIcon = icon;
        RefreshIcon(); 
    }

    public void RefreshIcon()
    {
        string icon = _defaultIcon;
        int highestPriority = -1;

        foreach(var pair in IconPriorities)
        {
            var data = pair.Value;
            if(data.priority > highestPriority)
            {
                icon = data.icon;
                highestPriority = data.priority;
            }
        }

        Thing.SetIcon(icon);
    }
}