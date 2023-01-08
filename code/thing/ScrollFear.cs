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

        int radius = 3;

        var things = user.ContainingGridManager.GetThingsWithinRange(user.GridPos, radius, allFlags: ThingFlags.Solid);
        foreach(var thing in things)
        {
            if (thing == user || thing.HasComponent<CFearful>())
                continue;

            if (thing.GetComponent<CActing>(out var acting))
            {
                var fearful = thing.AddComponent<CFearful>();
                fearful.Lifetime = 5f;
                fearful.FearedThing = user;

                ((CActing)acting).RefreshAction();
            }
        }

        var circlePoints = user.ContainingGridManager.GetPointsOnCircle(user.GridPos, radius);
        foreach (var point in circlePoints)
            RoguemojiGame.Instance.AddFloater("😱", point, Game.Random.Float(0.75f, 1.05f), user.CurrentLevelId, Vector2.Zero, new Vector2(0f, Game.Random.Float(-1f, -10f)), text: "", requireSight: false, EasingType.QuadOut, Game.Random.Float(0.35f, 0.45f), parent: null);

        base.Use(user);
    }
}
