using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class Book : Thing
{
    public override string AbilityName => "Read Book";
    public string SpellName { get; protected set; }

    public Book()
    {
        DisplayIcon = "📘";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;
        Flammability = 26;
    }

    public override HashSet<IntVector> GetAimingTargetCellsClient()
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        int radius = Math.Clamp(ThingWieldingThis.GetStatClamped(StatType.Intelligence), 1, 12);
        return Scroll.GetArenaAimingCells(radius, ThingWieldingThis);
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        int radius = Math.Clamp(ThingWieldingThis.GetStatClamped(StatType.Intelligence), 1, 12);
        return Scroll.IsPotentialArenaAimingCell(gridPos, radius, ThingWieldingThis);
    }
}
