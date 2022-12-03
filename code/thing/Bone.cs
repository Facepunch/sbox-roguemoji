using Sandbox;
using System;

namespace Roguemoji;
public partial class Bone : Thing
{
	public Bone()
	{
		DisplayIcon = "🦴";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Tooltip = "A bone.";
		Flags = ThingFlags.Selectable;
    }
}
