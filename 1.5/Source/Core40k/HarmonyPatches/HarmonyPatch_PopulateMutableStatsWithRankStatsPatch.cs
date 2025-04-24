using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(StatDef), "PopulateMutableStats")]
public class PopulateMutableStatsWithRankStatsPatch
{
    public static void Postfix(ref HashSet<StatDef> ___mutableStats)
    {
        foreach (var rank in DefDatabase<RankDef>.AllDefsListForReading)
        {
            if (rank.statFactors != null)
            {
                ___mutableStats.AddRange(rank.statFactors.Select((StatModifier mod) => mod.stat));
            }
            if (rank.statOffsets != null)
            {
                ___mutableStats.AddRange(rank.statOffsets.Select((StatModifier mod) => mod.stat));
            }
                
            foreach (var conditionalStatAffecter in rank.conditionalStatAffecters)
            {
                if (conditionalStatAffecter.statFactors != null)
                {
                    ___mutableStats.AddRange(conditionalStatAffecter.statFactors.Select((StatModifier mod) => mod.stat));
                }
                if (conditionalStatAffecter.statOffsets != null)
                {
                    ___mutableStats.AddRange(conditionalStatAffecter.statOffsets.Select((StatModifier mod) => mod.stat));
                }
            }
        }
    }
}