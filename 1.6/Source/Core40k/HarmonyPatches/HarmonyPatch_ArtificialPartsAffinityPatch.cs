using HarmonyLib;
using System;
using System.Collections.Generic;
using RimWorld;
using VEF.Genes;
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
    }
}