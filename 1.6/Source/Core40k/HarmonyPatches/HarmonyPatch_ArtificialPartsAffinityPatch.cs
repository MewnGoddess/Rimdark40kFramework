using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(PawnCapacityUtility), "CalculatePartEfficiency")]
public class ArtificialPartsAffinity
{
    public static void Postfix(ref float __result, HediffSet diffSet, BodyPartRecord part, ref List<PawnCapacityUtility.CapacityImpactor> impactors)
    {
        if (diffSet?.pawn == null)
        {
            return;
        }
        if (diffSet.HasDirectlyAddedPartFor(part))
        {
            var firstHediffMatchingPart = diffSet.GetFirstHediffMatchingPart<Hediff_AddedPart>(part);
            impactors?.Add(new PawnCapacityUtility.CapacityImpactorHediff
            {
                hediff = firstHediffMatchingPart
            });
            __result *= diffSet.pawn.GetStatValue(Core40kDefOf.BEWH_ArtificialPartsAffinityFactor);
        }
        else if (diffSet.IsBionicOrImplant(part.def))
        {
            var firstBionicOrImplant = Enumerable.FirstOrDefault(diffSet.hediffs, hediff => hediff.Part != null && hediff.Part.def == part.def && hediff.def.countsAsAddedPartOrImplant);
            impactors?.Add(new PawnCapacityUtility.CapacityImpactorHediff
            {
                hediff = firstBionicOrImplant
            });
            __result *= diffSet.pawn.GetStatValue(Core40kDefOf.BEWH_ArtificialPartsAffinityFactor);
        }
    }
}