using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollSentience : Scroll
{
    public ScrollSentience()
	{
        ScrollType = ScrollType.Sentience;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Animate a non-thinking object";
        Tooltip = "A scroll of Sentience";

        SetTattoo(Globals.Icon(IconType.Sentience));

        if (Game.IsServer)
        {
            AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        var thing = user.ContainingGridManager.GetThingsAt(targetGridPos).Where(x => x.Brain == null).Where(x => ScrollSentience.CanGainSentience(x)).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if(thing != null)
        {
            var brain = new SquirrelBrain();
            brain.ControlThing(thing);

            if (!thing.HasStat(StatType.Health)) thing.InitStat(StatType.Health, 10, 0, 10);
            if (!thing.HasStat(StatType.Sight)) thing.InitStat(StatType.Sight, 7);
            if (!thing.HasStat(StatType.Speed)) thing.InitStat(StatType.Speed, 5);
            if (!thing.HasStat(StatType.Attack)) thing.InitStat(StatType.Attack, 1);
            if (!thing.HasStat(StatType.Hearing)) thing.InitStat(StatType.Hearing, 3);
            if (!thing.HasStat(StatType.SightBlockAmount)) InitStat(StatType.SightBlockAmount, 3);

            if (!thing.HasFlag(ThingFlags.Solid)) thing.Flags = thing.Flags | ThingFlags.Solid;
            if (!thing.HasFlag(ThingFlags.CanWieldThings)) thing.Flags = thing.Flags | ThingFlags.CanWieldThings;
            if (!thing.HasFlag(ThingFlags.CanGainMutations)) thing.Flags = thing.Flags | ThingFlags.CanGainMutations;

            if (!thing.HasComponent<CTargeting>())
                thing.AddComponent<CTargeting>();

            if (!thing.HasComponent<CActing>())
            {
                var acting = thing.AddComponent<CActing>();
                acting.ActionDelay = CActing.CalculateActionDelay(thing.GetStatClamped(StatType.Speed));
                acting.ActionTimer = Game.Random.Float(0f, 1f);
            }

            thing.AddFloater(Globals.Icon(IconType.Sentience), time: 0f, new Vector2(0f, -5f), new Vector2(0f, -5f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.3f, scale: 0.6f, opacity: 1f);
        }

        user.ContainingGridManager.AddFloater(Globals.Icon(IconType.Sentience), targetGridPos, 0.8f, new Vector2(0f, 0f), new Vector2(0f, -14f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.2f, scale: 0.9f);

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();
    }

    public override HashSet<IntVector> GetAimingTargetCellsClient() 
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        int radius = Math.Clamp(ThingWieldingThis.GetStatClamped(StatType.Intelligence), 1, 10);
        return Scroll.GetArenaAimingCells(radius, ThingWieldingThis);
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        int radius = Math.Clamp(ThingWieldingThis.GetStatClamped(StatType.Intelligence), 1, 10);
        return Scroll.IsPotentialArenaAimingCell(gridPos, radius, ThingWieldingThis);
    }

    public static bool CanGainSentience(Thing thing)
    {
        if (thing is Hole)
            return false;

        return true;
    }
}
