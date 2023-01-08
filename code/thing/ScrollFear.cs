using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollFear : Thing
{
    public override string ChatDisplayIcons => $"📜{Globals.Icon(IconType.Fear)}";
    public override string AbilityName => "Read Scroll";

    public ScrollFear()
	{
		DisplayIcon = "📜";
        DisplayName = "Scroll of Fear";
        Description = "Scare all enemies near you";
        Tooltip = "A scroll of Fear";
        IconDepth = 0;
        Flags = ThingFlags.Selectable | ThingFlags.Useable;

        SetTattoo(Globals.Icon(IconType.Fear), scale: 0.5f, offset: new Vector2(1f, 0.5f), offsetWielded: new Vector2(0f, 0.3f), offsetInfo: new Vector2(8f, 5f), offsetCharWielded: new Vector2(2f, 0.5f), offsetInfoWielded: new Vector2(3f, 2f));

        if (Game.IsServer)
        {
            AddTrait(AbilityName, "🔥", "Sacrifice to cast the inscribed spell", offset: new Vector2(0f, -2f), tattooIcon: "📜", tattooScale: 0.45f, tattooOffset: new Vector2(0f, 4f));
        }
    }

    public override void Use(Thing user)
    {
        Destroy();

        base.Use(user);
    }
}
