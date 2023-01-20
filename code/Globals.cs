using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;

namespace Roguemoji;

public enum IconType { Blink, Teleport, Fear, Telekinesis, Poison, Blindness, Sleeping, Confusion, Hallucination, }
public enum VerbType { Use, Read }
public enum HallucinationTextType { Icon, Name, Tooltip, Description, GeneralDescription }

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
    private static List<HallucinationData> _hallucinations;
    private static List<string> _generalDescriptions;

    static Globals()
    {
        _hallucinations = new List<HallucinationData>()
        {
            new HallucinationData("🌲", "Tree", "A tree", "A tall evergreen tree", "Just a normal tree", "This would make a nice 🎄" ),
            new HallucinationData("🌳", "Tree", "A tree", "A tall deciduous tree", "Just a normal tree", "I want to hug it" ),
            new HallucinationData("💀", "Skull", "A skull", "This thing is dead", "A skeleton! 😱", "Does mine look like that?" ),
            new HallucinationData("😀", "Friend", "A friend", "Completely trustworthy", "Has never let me down", "I can always count on them" ),
            new HallucinationData("🐿️", "Squirrel", "A squirrel", "A bushy-tailed rodent", "Or is it a chipmunk?", "Are these things aggressive?" ),
            new HallucinationData("🏺", "Vase", "A vase", "Looks very fragile", "Probably a replica", "Look but don't touch" ),
            new HallucinationData("👽️", "Alien", "An alien", "Gotta get a photo", "Is it the probing kind?", "____" ),
            new HallucinationData("👻", "Ghost", "A ghost", "Spooky!", "Ghost", "Ghost" ),
            new HallucinationData("🐱‍🐉", "Dino-riding Cat", "A dino-riding cat", "What is going on?!", "Dino-riding Cat", "Dino-riding Cat" ),
            new HallucinationData("🤡", "Clown", "A clown", "🎈" , "Everyone loves clowns!", "Don't come any closer"),
            new HallucinationData("😈", "Demon", "A demon", "Am I in Hell?", "Begone!", "Possessed!"),
            new HallucinationData("👿", "Demon", "A demon", "Am I in Hell?", "Begone!", "It's angry at me"),
            new HallucinationData("👹", "Ogre", "An ogre", "Looks hostile!", "Aaahhh!!", "Beady eyes!"),
            new HallucinationData("🌛", "Moon", "A moon", "What's it doing down here?", "Made of 🧀?", "Where's the rest?"),
            new HallucinationData("🕷️", "Spider", "A spider", "Makes my skin crawl", "Spider", "Spider"),
            new HallucinationData("🐧", "Penguin", "A penguin", "Cute!", "Penguin", "Penguin"),
            new HallucinationData("💎", "Jewel", "A jewel", "It's huge!", "A flawless gemstone", "Jewel"),
            new HallucinationData("🚽", "Toilet", "A toilet", "Who left the seat open?", "Toilet", "Toilet"),
            new HallucinationData("⛄️", "Snowman", "A snowman", "It's alive!", "Snowman", "Snowman"),
            new HallucinationData("💣️", "Bomb", "A bomb", "Get down!!", "Bomb", "Bomb"),
            new HallucinationData("🔒️", "Lock", "A padlock", "I'll need a key for this", "Where is the key?", "Too strong to break"),
            new HallucinationData("👁️", "Eye", "An eye", "Stop looking at me!", "Eye", "Eye"),
            new HallucinationData("👀", "Eyes", "Some eyes", "They're watching me!", "Eyes", "Eyes"),
            new HallucinationData("🦷", "Tooth", "A tooth", "Whose is it?", "Tooth", "Tooth"),
            new HallucinationData("🤬", "Bastard", "A bastard", "Stop yelling at me!", "Bastard", "Bastard"),
            new HallucinationData("🐎", "Horse", "A horse", "Majestic!", "Horse", "Horse"),
            new HallucinationData("🦠", "Germ", "A germ", "Gross!", "Germ", "Germ" ),
            new HallucinationData("🐸", "Frog", "A frog", "Ribbit!", "Frog", "Frog"),
            new HallucinationData("🦉", "Owl", "An owl", "Hoot!", "Owl", "Owl"),
            new HallucinationData("🥩", "Meat", "Some meat", "🤤", "Meat", "Meat"),
            new HallucinationData("🕳️", "Hole", "A hole", "Where's it go?", "Hole", "Hole"),
            new HallucinationData("💩", "Poop", "A poop", "Nasty!", "Poop", "Poop"),
            new HallucinationData("⚠️", "Warning", "A warning", "Be careful!!", "Warning", "Warning"),
            new HallucinationData("💰", "Bag of Money", "A bag of money", "I'm gonna be rich", "Bag of Money", "Bag of Money"),
            new HallucinationData("🍆", "Eggplant", "An eggplant", "So suggestive!", "Eggplant", "Eggplant"),
            new HallucinationData("🍂", "Leaves", "A pile of leaves", "Ah, the melancholy colors of fall", "Leaves", "Leaves"),
            new HallucinationData("🌽", "Corncob", "A corncob", "____", "____", "____"),
            new HallucinationData("🍌", "Banana", "A banana", "Looks delicious!", "Banana", "Banana"),
            new HallucinationData("🎁", "Present", "A present", "For me?", "Present", "Present"),
            new HallucinationData("🥒", "Pickle", "A pickle", "It's been turned into a pickle", "Pickle", "Pickle"),
            new HallucinationData("🌈", "Rainbow", "A rainbow", "Beautiful!", "Rainbow", "Rainbow"),
        };

        _generalDescriptions = new List<string>()
        {
            "Testing teesting...",
            "Testing teesting... 2",
            "Testing teesting... 3",
        };
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
            case IconType.Hallucination: return "😵";
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
        var data = _hallucinations[inputCode % 41];

        if (textType == HallucinationTextType.Icon)
            return data.icon;
        else if(textType == HallucinationTextType.Name)
            return data.name;
        else if (textType == HallucinationTextType.Tooltip)
            return data.tooltip;
        else if (textType == HallucinationTextType.Description)
            return data.descriptions[strCode % 3];
        else if (textType == HallucinationTextType.GeneralDescription)
            return _generalDescriptions[inputCode % _generalDescriptions.Count];

        return "???";
    }
}
