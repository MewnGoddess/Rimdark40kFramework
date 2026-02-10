using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Genes40k;

public class CompProperties_DisableIfApparelCovers : CompProperties_AbilityEffect
{
    public List<BodyPartGroupDef> disabledIfCovered = [];
        
    public CompProperties_DisableIfApparelCovers()
    {
        compClass = typeof(Comp_DisableIfApparelCovers);
    }
}