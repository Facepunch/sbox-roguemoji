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
}

public partial class Thing : Entity
{
    [Net] public IList<Trait> Traits { get; private set; }

    public Trait AddTrait(string name, string icon, string description)
    {
        var trait = new Trait()
        {
            Name = name,
            Icon = icon,
            Description = description,
        };

        if (Traits == null)
            Traits = new List<Trait>();

        Traits.Add(trait);
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
