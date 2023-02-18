using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class MPoisonSpeed : Mutation
{
    public int SpeedAmount { get; private set; }
    private bool _didAddSpeed;

    public override void Init(Thing thing)
    {
        base.Init(thing);

        Trait = thing.AddTrait("Poison Rush", "🤩", $"Act more quickly while poisoned", offset: Vector2.Zero, tattooIcon: Globals.Icon(IconType.Poison), tattooScale: 0.4f, tattooOffset: new Vector2(-5.5f, -5f));

        SpeedAmount = 2;

        if (Thing.HasComponent<CPoisoned>())
            ApplyBoost();
    }

    public override void OnRemove()
    {
        base.OnRemove();

        if (_didAddSpeed)
            RemoveBoost();
    }

    void ApplyBoost()
    {
        Thing.AdjustStat(StatType.Speed, SpeedAmount);
        _didAddSpeed = true;
    }

    void RemoveBoost()
    {
        Thing.AdjustStat(StatType.Speed, -SpeedAmount);
        _didAddSpeed = false;
    }

    public override void OnAddComponent(TypeDescription type)
    {
        base.OnAddComponent(type);

        if (type == TypeLibrary.GetType(typeof(CPoisoned)))
            ApplyBoost();
    }

    public override void OnRemoveComponent(TypeDescription type)
    {
        base.OnRemoveComponent(type);

        if (type == TypeLibrary.GetType(typeof(CPoisoned)) && _didAddSpeed)
            RemoveBoost();
    }
}