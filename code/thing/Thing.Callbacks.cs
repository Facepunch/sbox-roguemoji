using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Thing : Entity
{
    /// <summary> Thing may be null. </summary>
    public virtual void OnWieldThing(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnWieldThing(thing); } }
    public virtual void OnNoLongerWieldingThing(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnNoLongerWieldingThing(thing); } }
      
    public virtual void OnWieldedBy(Thing thing)
    {
        ThingWieldingThis = thing;
        foreach (var component in ThingComponents) { component.Value.OnWieldedBy(thing); }
    }

    public virtual void OnNoLongerWieldedBy(Thing thing)
    {
        if (thing == ThingWieldingThis)
            ThingWieldingThis = null;

        foreach (var component in ThingComponents) { component.Value.OnNoLongerWieldedBy(thing); }
    }

    public virtual void OnChangedStat(StatType statType, int changeCurrent, int changeMin, int changeMax)
    {
        if (statType == StatType.Speed && GetComponent<CActing>(out var acting))
        {
            ((CActing)acting).ActionDelay = CActing.CalculateActionDelay(GetStatClamped(StatType.Speed));
        }
        else if (statType == StatType.Intelligence && HasStat(StatType.Mana))
        {
            int amount = changeCurrent * 1;
            AdjustStatMax(StatType.Mana, amount);
        }
        else if (statType == StatType.Stamina && HasStat(StatType.Energy))
        {
            int amount = changeCurrent * 2;
            AdjustStatMax(StatType.Energy, amount);
            StaminaDelay = Utils.Map(GetStatClamped(StatType.Stamina), 0, 20, 3f, 0.1f);
        }

        StatHash = 0;
        foreach (var pair in Stats)
            StatHash += pair.Value.HashCode;

        foreach (var component in ThingComponents)
            component.Value.OnChangedStat(statType, changeCurrent, changeMin, changeMax);
    }

    public virtual void OnSpawned() { }
    public virtual void OnEquipThing(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnEquipThing(thing); } }
    public virtual void OnUnequipThing(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnUnequipThing(thing); } }
    public virtual void OnEquippedTo(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnEquippedTo(thing); } }
    public virtual void OnUnequippedFrom(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnUnequippedFrom(thing); } }
    public virtual void OnActionRecharged() { foreach (var component in ThingComponents) { component.Value.OnActionRecharged(); } }
    public virtual void OnBumpedIntoThing(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnBumpedIntoThing(thing); } }
    public virtual void OnBumpedIntoBy(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnBumpedIntoBy(thing); } }
    public virtual void OnBumpedOutOfBounds(Direction dir) { foreach (var component in ThingComponents) { component.Value.OnBumpedOutOfBounds(dir); } }
    public virtual void OnMovedOntoThing(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnMovedOntoThing(thing); } }
    public virtual void OnMovedOntoBy(Thing thing) { foreach (var component in ThingComponents) { component.Value.OnMovedOntoBy(thing); } }
    public virtual void OnChangedGridPos() { foreach (var component in ThingComponents) { component.Value.OnChangedGridPos(); } }
    public virtual void OnAddComponent(TypeDescription type) { foreach (var component in ThingComponents) { component.Value.OnAddComponent(type); } }
    public virtual void OnRemoveComponent(TypeDescription type) { foreach (var component in ThingComponents) { component.Value.OnRemoveComponent(type); } }
    public virtual void OnCooldownStart() { foreach (var component in ThingComponents) { component.Value.OnCooldownStart(); } }
    public virtual void OnCooldownFinish() { foreach (var component in ThingComponents) { component.Value.OnCooldownFinish(); } }
    public virtual void OnFindTarget(Thing target) { foreach (var component in ThingComponents) { component.Value.OnFindTarget(target); } }
    public virtual void OnLoseTarget() { foreach (var component in ThingComponents) { component.Value.OnLoseTarget(); } }
    public virtual void OnPlayerChangedGridPos(RoguemojiPlayer player) { foreach (var component in ThingComponents) { component.Value.OnPlayerChangedGridPos(player); } }
    public virtual void OnDestroyed() { foreach (var component in ThingComponents) { component.Value.OnThingDestroyed(); } }
    public virtual void OnDied() { foreach (var component in ThingComponents) { component.Value.OnThingDied(); } }
}
