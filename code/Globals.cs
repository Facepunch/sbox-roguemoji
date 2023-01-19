﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;

namespace Roguemoji;

public enum IconType { Blink, Teleport, Fear, Telekinesis, Poison, Blindness, Sleeping, Confusion }
public enum VerbType { Use, Read }

public static class Globals
{
    public static string Icon(IconType iconType)
    {
        switch (iconType)
        {
            case IconType.Blink: return "✨";
            case IconType.Teleport: return "➰";
            case IconType.Fear: return "😱";
            case IconType.Telekinesis: return "🙌";
            case IconType.Poison: return "☠️";
            case IconType.Blindness: return "😑";
            case IconType.Sleeping: return "💤";
            case IconType.Confusion: return "🥴";
        }

        return "❓";
    }

    public static string GetStatReqString(StatType statType, int reqAmount, VerbType verbType)
    {
        string icon = Thing.GetStatIcon(statType);
        string verb = "";

        switch(verbType) 
        {
            case VerbType.Use: verb = "use"; break;
            case VerbType.Read: verb = "read"; break;
        }

        return $"You need {reqAmount}{icon} to {verb} this";
    }
}
