﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class SafetyVest : Thing
{
	public SafetyVest()
	{
		DisplayIcon = "🦺";
        DisplayName = "Safety Vest";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A very noticeable safety vest.";
		Flags = ThingFlags.Selectable | ThingFlags.Equipment;
    }
}
