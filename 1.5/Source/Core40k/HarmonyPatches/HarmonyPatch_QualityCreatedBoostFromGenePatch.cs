using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Core40k
{
    [HarmonyPatch(typeof(QualityUtility), "GenerateQualityCreatedByPawn")]
    public class QualityCreatedBoostFromGene
    {
        public static void Postfix(Pawn pawn, SkillDef relevantSkill, QualityCategory __result)
        {
            if (pawn.genes != null)
            {
                return;
            }

            var genes = pawn.genes.GenesListForReading.Where(g => 
                g.def.HasModExtension<DefModExtension_BoostQualityCreatedByPawn>() &&
                g.def.GetModExtension<DefModExtension_BoostQualityCreatedByPawn>().qualityBoostLevel.Keys.Contains(relevantSkill));

            if (!genes.Any())
            {
                return;
            }

            int levelIncrease = genes.Sum(g => g.def.GetModExtension<DefModExtension_BoostQualityCreatedByPawn>().qualityBoostLevel.Values.First());

            __result = (QualityCategory)Mathf.Min((int)__result + levelIncrease, 6);
        }
    }
}   