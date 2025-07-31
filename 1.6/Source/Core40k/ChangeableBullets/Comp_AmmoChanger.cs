using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k;

public class Comp_AmmoChanger : CompEquippable
{
    public CompProperties_AmmoChanger Props => (CompProperties_AmmoChanger)props;

    public List<ThingDef> AvailableProjectiles => Props.availableProjectiles;

    private ThingDef currentlySelectedProjectile;
    public ThingDef CurrentlySelectedProjectile
    {
        get => currentlySelectedProjectile ??= AvailableProjectiles.FirstOrFallback(def => HasResearchForAmmo(def, out _)) ?? PrimaryVerb.verbProps.defaultProjectile;
        set => currentlySelectedProjectile = value;
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
    
    public override IEnumerable<Gizmo> CompGetEquippedGizmosExtra()
    {
        foreach (var gizmo in base.CompGetEquippedGizmosExtra())
        {
            yield return gizmo;
        }
        
        var gizmo_AmmoChanger = new Gizmo_AmmoChanger(this);
        
        yield return gizmo_AmmoChanger;
    }
}