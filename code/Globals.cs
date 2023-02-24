using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;

namespace Roguemoji;

public enum IconType { Blink, Teleport, Fear, Telekinesis, Poison, Blindness, Sleeping, Confusion, Hallucination, Medicine, Mutation, Stunned, UnarmedAttack, Invisible, Polymorph, Displace, Confetti, Identify, Identified, Explosion,
    SacrificeScroll, Organize, Amnesia, Fire, Sentience, Water, Blood, Oil, Mud, Lava, Heal,
}
public enum VerbType { Use, Read }
public enum HallucinationTextType { Icon, Name, Tooltip, Description }
public enum PlayerIconPriority { Default, Move, ExitLevel, EnterLevel, AcademicCapNerd, SpeedIncrease, Invisible, GlassesOfPerception, Sunglasses, Organize, Confetti, Blinded, Poisoned, Attack, Confused, Fearful, 
    Hallucinating, Sleeping, HealOther, GainMutation, MudSad, BloodWet, WaterWet, EatReaction, NutAllergyReaction, TakeDamage, Stunned, RugbyCharge, Dead }
public enum IconDepthLevel { Puddle = 0, Hole = 1, Normal = 2, Solid = 5, Player = 6, Ghost = 7, Projectile = 8, Effect = 9 }
public enum SurfaceType { None, Grass, Dirt, Puddle, DeepWater, Concrete, }
public enum SoundActionType { Move, HitOther, GetHit, Drop, Throw, PickUp, PickUpInventory, PutDownInventory, Wield, Use, Destroyed }
public enum ThingSoundProfileType { Default, SmallCreature, MediumCreature, LargeCreature, Clothing, Scroll, Potion, Book, SmallLightObject, SmallHeavyObject, }

public struct HallucinationData
{
    public string icon;
    public string name;
    public string tooltip;
    public string[] descriptions;

    public HallucinationData(string icon, string name, string tooltip, string desc0, string desc1, string desc2)
    {
        this.icon = icon;
        this.name = name;
        this.tooltip = tooltip;
        this.descriptions = new string[3] { desc0, desc1, desc2 };
    }
}

public static class Globals
{
    public const int IGNITION_MAX = 100;
    public const float IGNITION_BRIGHTNESS_MAX = 13.0f;
    public const float IGNITION_COOL_DELAY = 0.25f;
    public const int IGNITION_COOL_AMOUNT = 2;
    public const int DEFAULT_THROW_DISTANCE = 5;

    private static List<HallucinationData> _hallucinations;

    static Globals()
    {
        CreateHallucinationData();
    }

    public static string Icon(IconType iconType)
    {
        switch (iconType)
        {
            case IconType.Blink: return "✨";
            case IconType.Teleport: return "➰";
            case IconType.Fear: return "😱";
            case IconType.Telekinesis: return "🙌";
            case IconType.Poison: return "☠️";
            case IconType.Blindness: return "🙈";
            case IconType.Sleeping: return "💤";
            case IconType.Confusion: return "❓";
            case IconType.Hallucination: return "🤪";
            case IconType.Medicine: return "💊";
            case IconType.Mutation: return "🧬";
            case IconType.Stunned: return "💫";
            case IconType.UnarmedAttack: return "✊";
            case IconType.Invisible: return "🦲";
            case IconType.Polymorph: return "🐑";
            case IconType.Displace: return "🌟";
            case IconType.Confetti: return "🎊";
            case IconType.Identify: return "🔍️";
            case IconType.Identified: return "💡";
            case IconType.Explosion: return "💥";
            case IconType.SacrificeScroll: return "💥";
            case IconType.Organize: return "🗃️";
            case IconType.Amnesia: return "🤷‍♂️";
            case IconType.Fire: return "🔥";
            case IconType.Sentience: return "👀";
            case IconType.Water: return "💧";
            case IconType.Blood: return "🩸";
            case IconType.Oil: return "⬛️";
            case IconType.Mud: return "🟫";
            case IconType.Lava: return "🌋";
            case IconType.Heal: return "💟";
        }

        return "";
    }

    public static string GetStatReqString(StatType statType, int reqAmount, VerbType verbType)
    {
        string icon = Thing.GetStatIcon(statType);
        string verb = "";

        switch(verbType) 
        {
            case VerbType.Use: verb = "use"; break;
            case VerbType.Read: verb = "read"; break;
        }

        return $"You need {reqAmount}{icon} to {verb} this";
    }

    public static string GetHallucinationText(string str, int seed, HallucinationTextType textType)
    {
        int strCode = string.IsNullOrEmpty(str) ? 0 : Math.Abs(str.GetHashCode());
        var inputCode = Math.Abs(strCode + seed);
        var data = _hallucinations[inputCode % _hallucinations.Count];

        if (textType == HallucinationTextType.Icon)
            return data.icon;
        else if(textType == HallucinationTextType.Name)
            return data.name;
        else if (textType == HallucinationTextType.Tooltip)
            return data.tooltip;
        else if (textType == HallucinationTextType.Description)
            return data.descriptions[strCode % 3];

        return "???";
    }

    private static void CreateHallucinationData()
    {
        _hallucinations = new List<HallucinationData>()
        {
            new HallucinationData("🌲", "Tree", "A tree", "A tall evergreen tree", "Just a normal tree", "This would make a nice 🎄" ),
            new HallucinationData("🌳", "Tree", "A tree", "A tall deciduous tree", "Just a normal tree", "Just a normal tree" ),
            new HallucinationData("💀", "Skull", "A skull", "This thing is dead", "A skeleton! 😱", "Does mine look like that?" ),
            new HallucinationData("😀", "Friend", "A friend", "Completely trustworthy", "Completely trustworthy", "I can always count on them" ),
            new HallucinationData("🐿️", "Squirrel", "A squirrel", "A bushy-tailed rodent", "A bushy-tailed rodent", "Or is it a chipmunk?" ),
            new HallucinationData("🏺", "Vase", "A vase", "Looks very fragile", "Looks very fragile", "Look but don't touch" ),
            new HallucinationData("👽️", "Alien", "An alien", "Gotta get a photo", "Gotta get a photo", "Is it the probing kind?" ),
            new HallucinationData("👻", "Ghost", "A ghost", "Spooky!", "Spooky!", "Spooky!" ),
            new HallucinationData("🎱", "Magic 8 Ball", "A magic 8 ball", "You may rely on it", "Ask again later", "Don't count on it" ),
            new HallucinationData("🤡", "Clown", "A clown", "🎈" , "🎈", "Everyone loves clowns!" ),
            new HallucinationData("😈", "Demon", "A demon", "Am I in Hell?", "Am I in Hell?", "Am I in Hell?"),
            new HallucinationData("🖼️", "Painting", "A painting", "It's very meaningful", "A true work of art", "A true work of art"),
            new HallucinationData("👹", "Ogre", "An ogre", "Looks hostile!", "It looks hostile", "Aaahhh!!" ),
            new HallucinationData("🌛", "Moon", "A moon", "What's it doing here?", "Made of 🧀?", "Where's the rest?"),
            new HallucinationData("🕷️", "Spider", "A spider", "Makes my skin crawl", "Makes my skin crawl", "Makes my skin crawl"),
            new HallucinationData("🧻", "Toilet paper", "A roll of toilet paper", "Might need this", "Might need this later", "4-ply! 😍" ),
            new HallucinationData("💎", "Jewel", "A jewel", "It's huge!", "A flawless gemstone", "A flawless gemstone"),
            new HallucinationData("🚽", "Toilet", "A toilet", "At least it's clean", "At least it's clean", "Scared to look inside"),
            new HallucinationData("⛄️", "Snowman", "A snowman", "It's alive!", "It's alive!", "It's alive!"),
            new HallucinationData("💣️", "Bomb", "A bomb", "Get down!", "Get down!", "Get down!!"),
            new HallucinationData("🔒️", "Lock", "A padlock", "I don't have the key", "Where is the key?", "Too strong to break"),
            new HallucinationData("👁️", "Eye", "An eye", "Stop looking at me!", "Stop watching me!", "It's watching me"),
            new HallucinationData("👀", "Eyes", "Some eyes", "They're watching me!", "It sees me", "It sees me"),
            new HallucinationData("🦷", "Tooth", "A tooth", "Whose is it?", "Whose is it?", "It's big"),
            new HallucinationData("🤬", "Bastard", "A bastard", "Stop yelling at me!", "Stop shouting at me!", "Shut up!" ),
            new HallucinationData("🐎", "Horse", "A horse", "Majestic!", "Majestic!", "Majestic!"),
            new HallucinationData("🦠", "Germ", "A germ", "Gross!", "Gross!", "Yuck!" ),
            new HallucinationData("🐸", "Frog", "A frog", "Ribbit!", "Ribbit!", "Ribbit!"),
            new HallucinationData("🦉", "Owl", "An owl", "Hoot!", "Hoot!", "Hoot! Hoot!"),
            new HallucinationData("🥩", "Meat", "Some meat", "🤤", "🤤", "Extra rare"),
            new HallucinationData("🕳️", "Hole", "A hole", "Where's it go?", "Where's it go?", "Where's it go?"),
            new HallucinationData("💩", "Poop", "A poop", "Nasty!", "Nasty!", "Nasty!"),
            new HallucinationData("⚠️", "Warning", "A warning", "Be careful!", "Be careful!!", "Look out!"),
            new HallucinationData("💰", "Bag of Money", "A bag of money", "I'm gonna be rich", "I'm gonna be rich", "I'm gonna be rich"),
            new HallucinationData("📡", "Satellite Dish", "A satellite dish", "Receiving communications from space", "Receiving communications from space!", "Receiving communications from space" ),
            new HallucinationData("🍂", "Leaves", "A pile of leaves", "Small pile of dry leaves", "Small pile of dry leaves", "The first leaves of fall"),
            new HallucinationData("🌽", "Corncob", "A corncob", "An unsalted corncob", "An unsalted corncob", "A salted corncob"),
            new HallucinationData("🍌", "Banana", "A banana", "Perfectly ripe", "Perfectly ripe", "Perfectly ripe"),
            new HallucinationData("🎁", "Present", "A present", "For me?", "For me?", "For whom?"),
            new HallucinationData("🥒", "Pickle", "A pickle", "It's been turned into a pickle", "A sour, crunchy pickle", "A sour, crunchy pickle"),
            new HallucinationData("🌈", "Rainbow", "A rainbow", "Beautiful!", "Beautiful!", "Beautiful!"),
            new HallucinationData("🎲", "Die", "A single die", "It landed on 1", "It landed on 1", "It landed on 1"),
            new HallucinationData("🆗", "The Word OK", "The word OK", "It's gonna be okay", "It's gonna be okay", "It's gonna be okay"),
            new HallucinationData("🥓", "Bacon", "Some bacon", "Two crispy strips of bacon", "Two crispy strips of bacon", "Two crispy strips of bacon"),
            new HallucinationData("🐘", "Elephant", "An elephant", "A massive lumbering elephant", "A massive lumbering elephant", "A massive lumbering elephant"),
        };
    }
}
