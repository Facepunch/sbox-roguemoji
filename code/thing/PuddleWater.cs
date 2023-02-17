using Sandbox;
using System;
using System.Linq;

namespace Roguemoji;
public partial class PuddleWater : Thing
{
    private float _elapsedTime;
    private int _iconState;

	public PuddleWater()
	{
		DisplayIcon = "💧";
        DisplayName = "Puddle of Water";
        Description = "The ground is covered in a layer of water";
        Tooltip = "A puddle of water";
        IconDepth = (int)IconDepthLevel.Normal;
        ShouldUpdate = true;
        Flags = ThingFlags.Selectable | ThingFlags.Puddle;
        Flammability = 0;
    }

    // todo: make splashing noise when you move onto it
    // todo: make visible when walking onto this while invisible

    public override void Update(float dt)
    {
        base.Update(dt);

        _elapsedTime += dt;

        if(_iconState == 0 && _elapsedTime > 0.25f)
        {
            _iconState++;
            DisplayIcon = "🔵";
            IconDepth = (int)IconDepthLevel.Puddle;
        }
        else if(_iconState == 1 && _elapsedTime > 0.4f)
        {
            _iconState++;
            DisplayIcon = "🟦";
            ShouldUpdate = false;
        }
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);

        // todo: wet component?
        DouseFire(thing);
    }

    public override void OnMovedOntoThing(Thing thing)
    {
        base.OnMovedOntoThing(thing);

        DouseFire(thing);
    }

    public static void DouseFire(Thing thing)
    {
        CBurning burning = null;
        if (thing.GetComponent<CBurning>(out var component))
            burning = (CBurning)component;

        CProjectile projectile = null;
        if (thing.GetComponent<CProjectile>(out var component2))
            projectile = (CProjectile)component2;

        if (burning != null && projectile == null)
            burning.Remove();

        thing.IgnitionAmount = 0;

        var player = thing.Brain as RoguemojiPlayer;
        if (player == null)
            return;

        foreach(var item in player.InventoryGridManager.GetAllThings().Where(x => x.HasComponent<CBurning>()))
        {
            if (item.GetComponent<CBurning>(out var itemComponent))
            {
                var itemBurning = (CBurning)itemComponent;
                itemBurning.Remove();
                item.IgnitionAmount = 0;
            }
        }
    }
}
