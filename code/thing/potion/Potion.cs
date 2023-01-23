using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public enum PotionType { Health, Mana, Energy, Poison, Blindness, Sleeping, Confusion, Hallucination, Speed, Medicine, Mutation, }
public partial class Potion : Thing
{
    [Net] public PotionType PotionType { get; protected set; }
    public override string ChatDisplayIcons => GetChatDisplayIcons(PotionType);
    public virtual string SplashIcon => "";

    public Potion()
    {
        DisplayIcon = "🧉";
        IconDepth = 0;
    }

    public static string GetDisplayName(PotionType potionType)
    {
        switch(potionType)
        {
            case PotionType.Energy: return "Energy Potion";
            case PotionType.Health: return "Health Potion";
            case PotionType.Mana: return "Mana Potion";
            case PotionType.Poison: return "Poison Potion";
            case PotionType.Blindness: return "Blindness Potion";
            case PotionType.Sleeping: return "Sleeping Potion";
            case PotionType.Confusion: return "Confusion Potion";
            case PotionType.Speed: return "Speed Potion";
            case PotionType.Medicine: return "Medicine Potion";
            case PotionType.Mutation: return "Mutation Potion";
        }

        return "";
    }

    public static string GetChatDisplayIcons(PotionType potionType)
    {
        switch (potionType)
        {
            case PotionType.Energy: return $"🧉{GetStatIcon(StatType.Energy)}";
            case PotionType.Health: return $"🧉{GetStatIcon(StatType.Health)}";
            case PotionType.Mana: return $"🧉{GetStatIcon(StatType.Mana)}";
            case PotionType.Poison: return $"🧉{Globals.Icon(IconType.Poison)}";
            case PotionType.Blindness: return $"🧉{Globals.Icon(IconType.Blindness)}";
            case PotionType.Sleeping: return $"🧉{Globals.Icon(IconType.Sleeping)}";
            case PotionType.Confusion: return $"🧉{Globals.Icon(IconType.Confusion)}";
            case PotionType.Speed: return $"🧉{Globals.Icon(IconType.Speed)}";
            case PotionType.Medicine: return $"🧉{Globals.Icon(IconType.Medicine)}";
            case PotionType.Mutation: return $"🧉{Globals.Icon(IconType.Mutation)}";
        }

        return "🧉";
    }

    public override void Use(Thing user)
    {
        base.Use(user);
        RoguemojiGame.Instance.RevealPotion(PotionType, user.GridPos, user.CurrentLevelId);
    }

    public void SetTattoo(string icon)
    {
        SetTattoo(icon, scale: 0.475f, offset: new Vector2(-0.858505f, 6f), offsetWielded: new Vector2(-1.5f, 6f), offsetInfo: new Vector2(-3.5f, 16f), offsetCharWielded: new Vector2(-2f, 6f), offsetInfoWielded: new Vector2(-4.75f, 6.25f));
    }

    public override void HitOther(Thing target, Direction direction)
    {
        base.HitOther(target, direction);

        if(HasComponent<CProjectile>())
            Break(target.GridPos);
    }

    public void Break(IntVector breakGridPos)
    {
        for(int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var gridPos = breakGridPos + new IntVector(x, y);
                if (ContainingGridManager.IsGridPosInBounds(gridPos))
                {
                    RoguemojiGame.Instance.AddFloater(SplashIcon, gridPos, Game.Random.Float(0.7f, 0.9f), CurrentLevelId, new Vector2(0f, 0f), new Vector2(0f, Game.Random.Float(-10f, -15f)), height: 0f, text: "", requireSight: false, alwaysShowWhenAdjacent: true, 
                        EasingType.QuadOut, fadeInTime: Game.Random.Float(0.01f, 0.05f), scale: Game.Random.Float(0.75f, 0.9f), opacity: 0.4f);

                    ApplyEffectToGridPos(gridPos);

                    foreach (var thing in ContainingGridManager.GetThingsAt(gridPos))
                        ApplyEffectToThing(thing);
                }
            }
        }

        RoguemojiGame.Instance.RevealPotion(PotionType, breakGridPos, CurrentLevelId);

        Destroy();
    }

    public virtual void ApplyEffectToThing(Thing thing) { }
    public virtual void ApplyEffectToGridPos(IntVector gridPos) { }
}
