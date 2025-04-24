using HarmonyLib;
using System;
using VanillaGenesExpanded;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
public class GenderDistributionPatch
{
    public static void Postfix(Pawn pawn)
    {
        if (pawn.genes == null)
        {
            return;
        }
        var genesListForReading = pawn.genes.GenesListForReading;
        float male = -1;
        float female = -1;
        foreach (var gene in genesListForReading)
        {
            if (!gene.Active) continue;
                
            var modExtension = gene.def.GetModExtension<GeneExtension>();
            if (modExtension != null && (modExtension.forceFemale || modExtension.forceMale))
            {
                return;
            }
            var defModExtension = gene.def.GetModExtension<DefModExtension_GenderDistribution>();
            if (defModExtension == null) continue;
                
            male = defModExtension.male;
            female = defModExtension.female;
        }

        if (!(male >= 0) || !(female >= 0)) return;
            
        var rand = new Random();
        var rando = rand.Next(1, 100);
        pawn.gender = rando <= male ? Gender.Male : Gender.Female;

    }
}