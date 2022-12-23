using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Trait : BaseNetworkable
{
    [Net] public string Name { get; set; }
    [Net] public string Icon { get; set; }
    [Net] public string Description { get; set; }
    [Net] public Color BackgroundColor { get; set; }
    [Net] public float Progress { get; set; }
    [Net] public string Source { get; set; }
    [Net] public bool HasTattoo { get; set; }
    [Net] public string TattooIcon { get; set; }
    [Net] public float TattooScale { get; set; }
    [Net] public Vector2 TattooOffset { get; set; }

    public void SetTattoo(string icon, float scale, Vector2 offset)
    {
        HasTattoo = true;
        TattooIcon = icon;
        TattooScale = scale;
        TattooOffset = offset;
    }
}

public partial class Thing : Entity
{
    [Net] public IList<Trait> Traits { get; private set; }

    public Trait AddTrait(string name, string icon, string description, string source = "")
    {
        var trait = new Trait()
        {
            Name = name,
            Icon = icon,
            Description = description,
            Source = source,
        };

        if (Traits == null)
            Traits = new List<Trait>();

        Traits.Add(trait);
        return trait;
    }

    public Trait AddTrait(string name, string icon, string description, string tattooIcon, float tattooScale, Vector2 tattooOffset, string source = "")
    {
        Trait trait = AddTrait(name, icon, description, source);
        trait.SetTattoo(tattooIcon, tattooScale, tattooOffset);
        return trait;
    }

    public void RemoveTrait(Trait trait)
    {
        if (Traits.Contains(trait))
            Traits.Remove(trait);
    }

    public void ClearTraits()
    {
        Traits.Clear();
    }
}
