using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Sandbox;

namespace Roguemoji;

public struct GridFloaterData
{
    public string icon;
    public IntVector gridPos;
    public float time;
    public TimeSince timeSinceStart;
    public string text;
    public bool requireSight;
    public bool alwaysShowWhenAdjacent;
    public Vector2 offsetStart;
    public Vector2 offsetEnd;
    public float height;
    public EasingType offsetEasingType;
    public float fadeInTime;
    public float scale;
    public float opacity;
    public float shakeAmount;

    public GridFloaterData(string icon, IntVector gridPos, float time, Vector2 offsetStart, Vector2 offsetEnd, float height, string text, bool requireSight, bool alwaysShowWhenAdjacent,
        EasingType offsetEasingType, float fadeInTime, float scale, float opacity, float shakeAmount)
    {
        this.icon = icon;
        this.gridPos = gridPos;
        this.time = time;
        this.timeSinceStart = 0f;
        this.offsetStart = offsetStart;
        this.offsetEnd = offsetEnd;
        this.height = height;
        this.text = text;
        this.requireSight = requireSight;
        this.alwaysShowWhenAdjacent = alwaysShowWhenAdjacent;
        this.offsetEasingType = offsetEasingType;
        this.fadeInTime = fadeInTime;
        this.scale = scale;
        this.opacity = opacity;
        this.shakeAmount = shakeAmount;
    }
}

public partial class GridManager : Entity
{
    public List<GridFloaterData> Floaters { get; private set; } // Client-only

    public void HandleFloaters(float dt)
    {
        for (int i = Floaters.Count - 1; i >= 0; i--)
        {
            var floater = Floaters[i];
            if (floater.time > 0f && floater.timeSinceStart > floater.time)
                Floaters.RemoveAt(i);
        }
    }

    //public List<GridFloaterData> GetFloaters(IntVector gridPos)
    //{
    //    var floaters = new List<GridFloaterData>();

    //    foreach(var f in Floaters)
    //    {
    //        if(f.gridPos.Equals(gridPos))
    //            floaters.Add
    //    }
    //}

    public void AddFloater(string icon, IntVector gridPos, float time, Vector2 offsetStart, Vector2 offsetEnd, float height = 0f, string text = "", bool requireSight = true, bool alwaysShowWhenAdjacent = false,
                        EasingType offsetEasingType = EasingType.Linear, float fadeInTime = 0f, float scale = 1f, float opacity = 1f, float shakeAmount = 0f)
    {
        AddFloaterClient(icon, gridPos, time, offsetStart, offsetEnd, height, text, requireSight, alwaysShowWhenAdjacent, offsetEasingType, fadeInTime, scale, opacity, shakeAmount);
    }

    [ClientRpc]
    public void AddFloaterClient(string icon, IntVector gridPos, float time, Vector2 offsetStart, Vector2 offsetEnd, float height = 0f, string text = "", bool requireSight = true, bool alwaysShowWhenAdjacent = false,
                        EasingType offsetEasingType = EasingType.Linear, float fadeInTime = 0f, float scale = 1f, float opacity = 1f, float shakeAmount = 0f)
    {
        Floaters.Add(new GridFloaterData(icon, gridPos, time, offsetStart, offsetEnd, height, text, requireSight, alwaysShowWhenAdjacent, offsetEasingType, fadeInTime, scale, opacity, shakeAmount));
    }

    public void RemoveFloater(string icon, IntVector gridPos)
    {
        RemoveFloaterClient(icon, gridPos);
    }

    [ClientRpc]
    public void RemoveFloaterClient(string icon, IntVector gridPos)
    {
        for (int i = Floaters.Count - 1; i >= 0; i--)
        {
            var floater = Floaters[i];
            if (floater.gridPos.Equals(gridPos) && floater.icon.Equals(icon))
                Floaters.RemoveAt(i);
        }
    }
}
