using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Thing : Entity
{
    public Dictionary<TypeDescription, ThingComponent> ThingComponents = new Dictionary<TypeDescription, ThingComponent>();

    public ThingComponent AddComponent(TypeDescription type)
    {
        if (type == null)
        {
            Log.Info("type is null!");
            return null;
        }

        ThingComponent component = null;

        if (ThingComponents.ContainsKey(type))
        {
            component = ThingComponents[type];
            component.ReInitialize();
        }
        else
        {
            component = type.Create<ThingComponent>();
            component.Init(this);
            ThingComponents.Add(type, component);
        }

        OnAddComponent(type);
        return component;
    }

    public T AddComponent<T>() where T : ThingComponent
    {
        return AddComponent(TypeLibrary.GetType(typeof(T))) as T;
    }

    public void RemoveComponent(TypeDescription type)
    {
        if (ThingComponents.ContainsKey(type))
        {
            var component = ThingComponents[type];
            component.OnRemove();
            ThingComponents.Remove(type);
            OnRemoveComponent(type);
        }
    }

    public void RemoveComponent<T>() where T : ThingComponent
    {
        RemoveComponent(TypeLibrary.GetType(typeof(T)));
    }

    public bool GetComponent(TypeDescription type, out ThingComponent component)
    {
        if (ThingComponents.ContainsKey(type))
        {
            component = ThingComponents[type];
            return true;
        }

        component = null;
        return false;
    }

    public bool GetComponent<T>(out ThingComponent component) where T : ThingComponent
    {
        return GetComponent(TypeLibrary.GetType(typeof(T)), out component);
    }

    public bool HasComponent(TypeDescription type)
    {
        return ThingComponents.ContainsKey(type);
    }

    /// <summary> Server-only. </summary>
    public bool HasComponent<T>() where T : ThingComponent
    {
        return HasComponent(TypeLibrary.GetType(typeof(T)));
    }

    public void ForEachComponent(Action<ThingComponent> action)
    {
        foreach (var (_, component) in ThingComponents)
        {
            action(component);
        }
    }

    [ClientRpc]
    public void VfxNudge(Direction direction, float lifetime, float distance)
    {
        RemoveMoveVfx();

        var nudge = AddComponent<VfxNudge>();
        nudge.Direction = direction;
        nudge.Lifetime = lifetime;
        nudge.Distance = distance;
    }

    [ClientRpc]
    public void VfxSlide(Direction direction, float lifetime, float distance)
    {
        RemoveMoveVfx();

        var slide = AddComponent<VfxSlide>();
        slide.Direction = direction;
        slide.Lifetime = lifetime;
        slide.Distance = distance;
    }

    [ClientRpc]
    public void VfxShake(float lifetime, float distance)
    {
        var shake = AddComponent<VfxShake>();
        shake.Lifetime = lifetime;
        shake.Distance = distance;
    }

    [ClientRpc]
    public void VfxScale(float lifetime, float startScale, float endScale)
    {
        var scale = AddComponent<VfxScale>();
        scale.Lifetime = lifetime;
        scale.StartScale = startScale;
        scale.EndScale = endScale;
    }

    [ClientRpc]
    public void VfxSpin(float lifetime, float startAngle, float endAngle)
    {
        var scale = AddComponent<VfxSpin>();
        scale.Lifetime = lifetime;
        scale.StartAngle = startAngle;
        scale.EndAngle = endAngle;
    }

    [ClientRpc]
    public void VfxFly(IntVector startingGridPos, float lifetime, float heightY = 0f, EasingType progressEasingType = EasingType.ExpoOut, EasingType heightEasingType = EasingType.QuadInOut)
    {
        RemoveMoveVfx();

        var fly = AddComponent<VfxFly>();
        fly.StartingGridPos = startingGridPos;
        fly.Lifetime = lifetime;
        fly.HeightY = heightY;
        fly.ProgressEasingType = progressEasingType;
        fly.HeightEasingType = heightEasingType;
    }

    [ClientRpc]
    public void VfxOpacityLerp(float lifetime, float startOpacity, float endOpacity, EasingType easingType = EasingType.Linear)
    {
        var opacityLerp = AddComponent<VfxOpacityLerp>();
        opacityLerp.Lifetime = lifetime;
        opacityLerp.StartOpacity = startOpacity;
        opacityLerp.EndOpacity = endOpacity;
        opacityLerp.EasingType = easingType;
    }

    void RemoveMoveVfx()
    {
        RemoveComponent<VfxSlide>();
        RemoveComponent<VfxNudge>();
        RemoveComponent<VfxFly>();
    }
}
