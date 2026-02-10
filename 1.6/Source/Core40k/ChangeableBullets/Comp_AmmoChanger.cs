using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k;

public class Comp_AmmoChanger : ThingComp
{
    public CompProperties_AmmoChanger Props => (CompProperties_AmmoChanger)props;

    public CompEquippable Equippable => parent.GetComp<CompEquippable>();
    public List<ThingDef> AvailableProjectiles => Props.availableProjectiles;

    public Pawn pawn => (parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn;
    public ThingWithComps Weapon => parent;
    
    private ThingDef nextProjectile;
    private ThingDef currentlySelectedProjectile;
    public ThingDef CurrentlySelectedProjectile => currentlySelectedProjectile ??= AvailableProjectiles?.FirstOrFallback(def => HasResearchForAmmo(def, out _)) ?? Equippable.PrimaryVerb.verbProps.defaultProjectile;
    
    public DefModExtension_AmmoChanger DefModExtensionAmmoChanger => CurrentlySelectedProjectile?.GetModExtension<DefModExtension_AmmoChanger>();
    public int ShotsPerBurst => DefModExtensionAmmoChanger?.shotsPerBurst ?? Weapon.def.Verbs.FirstOrDefault()?.burstShotCount ?? 0;
    public float EffectiveRange => DefModExtensionAmmoChanger?.effectiveRange ?? Weapon.def.Verbs.FirstOrDefault()?.range ?? 0;
    public float WarmupTime => DefModExtensionAmmoChanger?.warmupTime ?? Weapon.def.Verbs.FirstOrDefault()?.warmupTime ?? 0;
    
    public void LoadNextProjectile()
    {
        currentlySelectedProjectile = nextProjectile;
        nextProjectile = null;
    }

    public void SetNextProjectile(ThingDef projectile)
    {
        nextProjectile = projectile;
    }
    
    public bool HasResearchForAmmo(ThingDef ammoDef, out ResearchProjectDef researchDef)
    {
        if (!ammoDef.HasModExtension<DefModExtension_AmmoChanger>())
        {
            researchDef = null;
            return true;
        }

        var research = ammoDef.GetModExtension<DefModExtension_AmmoChanger>().unlockedBy;
        researchDef = research;
        return research?.IsFinished ?? true;
    }

    public override void PostExposeData()
    {
        Scribe_Defs.Look(ref currentlySelectedProjectile, "currentlySelectedProjectile");
        Scribe_Defs.Look(ref nextProjectile, "nextProjectile");
        base.PostExposeData();
    }
}