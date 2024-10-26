using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k
{
    [HarmonyPatch(typeof(MassUtility), "Capacity")]
    public class AddedWorldCarryCapacity
    {
        public static void Postfix(ref float __result, Pawn p)
        {
            if (__result == 0)
            {
                return;
            }
            if (p.genes == null)
            {
                return;
            }
            var genes = p.genes.GenesListForReading.Where(x => x.def.HasModExtension<DefModExtension_GeneExtension>());
            if (genes.EnumerableNullOrEmpty())
            {
                return;
            }
            
            var num = genes.Sum(gene => gene.def.GetModExtension<DefModExtension_GeneExtension>().addedWorldCarryCapacity);

            __result += num;
        }
    }
}   