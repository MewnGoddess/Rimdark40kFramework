using HarmonyLib;
using RimWorld;
using Verse;

namespace Core40k
{
    [HarmonyPatch(typeof(PawnCapacityWorker_Breathing), "CalculateCapacityLevel")]
    public class LostLungSurvivalPatch
    {
        public static float Postfix(float originalResult, HediffSet __0)
        {
            var hediffSet = __0;

            var pawn = hediffSet.pawn;
            if (pawn.DestroyedOrNull() || pawn.genes == null || pawn.Dead)
            {
                return originalResult;
            }
            foreach (var gene in pawn.genes.GenesListForReading)
            {
                if (!gene.def.HasModExtension<DefModExtension_LostLungSurvival>()) continue;
                
                var result = originalResult + 0.4f;
                return result;
            }
            return originalResult;
        }
    }
}   