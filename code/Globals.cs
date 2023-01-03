using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;

namespace Roguemoji;

public enum IconType { Blink, Teleport }

public static class Globals
{
    public static string Icon(IconType iconType)
    {
        switch (iconType)
        {
            case IconType.Blink: return "✨";
            case IconType.Teleport: return "➰";
        }

        return "❓";
    }
}
