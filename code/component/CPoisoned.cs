using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CPoisoned : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }

    public float HurtTimer { get; set; }
    public float HurtDelay { get; set; }
    public int Level { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        HurtDelay = 3f;
        Level = 1;

        Trait = thing.AddTrait("Poisoned", Globals.Icon(IconType.Poison), GetTraitDescription(), offset: Vector2.Zero, labelText: "", labelFontSize: 18, labelOffset: new Vector2(0f, -12f), labelColor: new Color(0.2f, 1f, 0.2f));

        thing.AddFloater(Globals.Icon(IconType.Poison), time: 0f, new Vector2(-14f, -14f), Vector2.Zero, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.025f, scale: 0.5f, opacity: 0.25f);

        if (thing is RoguemojiPlayer && thing.GetComponent<CIconPriority>(out var component))
            IconId = ((CIconPriority)component).AddIconPriority("🤒", (int)PlayerIconPriority.Poisoned);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        HurtTimer += dt;
        if (HurtTimer > HurtDelay)
        {
            HurtTimer -= HurtDelay;

            Thing.Hurt(Level, showImpactFloater: false);

            var offset = new Vector2(Game.Random.Float(-5f, 4f), Game.Random.Float(-5f, 4f));
            var scale = Utils.Map(TimeElapsed, 0f, Lifetime, 0.8f, 0.25f, EasingType.QuadIn);
            var opacity = Utils.Map(TimeElapsed, 0f, Lifetime, 0.75f, 0.4f);
            Thing.AddFloater(Globals.Icon(IconType.Poison), 0.25f, offset, offset, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.SineIn, fadeInTime: 0.025f, scale: scale, opacity: opacity);
        }

        TimeElapsed += dt;
        if(Lifetime > 0f && TimeElapsed > Lifetime)
        {
            Level--;
            Trait.Description = GetTraitDescription();

            if (Level <= 0)
            {
                Remove();
                return;
            }
        }

        Trait.BarPercent = 1f - Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f);
    }

    public override void ReInitialize()
    {
        base.ReInitialize();

        Level++;
        //HurtTimer = 0f;
        Trait.Description = GetTraitDescription();
        UpdateLabel();
    }

    string GetTraitDescription()
    {
        return $"Lose {Level}{Thing.GetStatIcon(StatType.Health)} every 3 seconds";
    }

    void UpdateLabel()
    {
        if (Level == 1)
        {
            Trait.HasLabel = false;
        }
        else
        {
            Trait.HasLabel = true;
            Trait.LabelText = $"{Level}";
        }
    }

    public override void OnRemove()
    {
        Thing.RemoveTrait(Trait);
        RoguemojiGame.Instance.RemoveFloater(Globals.Icon(IconType.Poison), Thing.CurrentLevelId, parent: Thing);

        if (Thing is RoguemojiPlayer && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);
    }

    public override void OnThingDestroyed()
    {
        RoguemojiGame.Instance.RemoveFloater(Globals.Icon(IconType.Poison), Thing.CurrentLevelId, parent: Thing);
    }

    public override void OnThingDied()
    {
        Remove();
    }
}