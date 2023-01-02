using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Trait : Entity
{
    [Net] public string TraitName { get; set; }
    [Net] public string Icon { get; set; }
    [Net] public string Description { get; set; }
    [Net] public Color BackgroundColor { get; set; }
    [Net] public float Progress { get; set; }
    [Net] public string Source { get; set; }
    [Net] public Vector2 Offset { get; set; }

    [Net] public bool HasTattoo { get; set; }
    [Net] public string TattooIcon { get; set; }
    [Net] public float TattooScale { get; set; }
    [Net] public Vector2 TattooOffset { get; set; }
    
    [Net] public bool HasLabel { get; set; }
    [Net] public string LabelText { get; set; }
    [Net] public int LabelFontSize { get; set; }
    [Net] public Vector2 LabelOffset { get; set; }
    [Net] public Color LabelColor { get; set; }

    public Trait()
    {
        Transmit = TransmitType.Always;
    }

    public void SetTattoo(string icon, float scale, Vector2 offset)
    {
        HasTattoo = true;
        TattooIcon = icon;
        TattooScale = scale;
        TattooOffset = offset;
    }

    public void SetLabel(string text, int fontSize, Vector2 offset, Color color)
    {
        HasLabel = true;
        LabelText = text;
        LabelFontSize = fontSize;
        LabelOffset = offset;
        LabelColor = color;
    }
}

public partial class Thing : Entity
{
    [Net] public IList<Trait> Traits { get; private set; }

    public Trait AddTrait(string name, string icon, string description, Vector2 offset, string source = "")
    {
        var trait = new Trait()
        {
            TraitName = name,
            Icon = icon,
            Description = description,
            Offset = offset,
            Source = source,
        };

        if (Traits == null)
            Traits = new List<Trait>();

        Traits.Add(trait);
        return trait;
    }

    public Trait AddTrait(string name, string icon, string description, Vector2 offset, string tattooIcon, float tattooScale, Vector2 tattooOffset, string source = "")
    {
        Trait trait = AddTrait(name, icon, description, offset, source);
        trait.SetTattoo(tattooIcon, tattooScale, tattooOffset);
        return trait;
    }

    public Trait AddTrait(string name, string icon, string description, Vector2 offset, string tattooIcon, float tattooScale, Vector2 tattooOffset, string labelText, int labelFontSize, Vector2 labelOffset, Color labelColor, string source = "")
    {
        Trait trait = AddTrait(name, icon, description, offset, source);
        trait.SetTattoo(tattooIcon, tattooScale, tattooOffset);
        trait.SetLabel(labelText, labelFontSize, labelOffset, labelColor);
        return trait;
    }

    public Trait AddTrait(string name, string icon, string description, Vector2 offset, string labelText, int labelFontSize, Vector2 labelOffset, Color labelColor, string source = "")
    {
        Trait trait = AddTrait(name, icon, description, offset, source);
        trait.SetLabel(labelText, labelFontSize, labelOffset, labelColor);
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
