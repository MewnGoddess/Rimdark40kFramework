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
            IEnumerable<Gene> genes = p.genes.GenesListForReading.Where(x => x.def.HasModExtension<DefModExtension_GeneExtension>());
            if (genes.EnumerableNullOrEmpty())
            {
                return;
            }
            float num = 0;

            foreach (Gene gene in genes)
            {
                num += gene.def.GetModExtension<DefModExtension_GeneExtension>().addedWorldCarryCapacity;
            }

            __result += num;
        }
    }
}   