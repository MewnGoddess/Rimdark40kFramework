using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(StatWorker), "GetExplanationUnfinalized")]
public static class GetExplanationUnfinalizedFromRankPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions.ToList();
        var patched = false;
        for (var i = 0; i < codeInstructions.Count; i++)
        {
            if (!patched && codeInstructions[i+1].opcode == OpCodes.Ret)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetExplanationUnfinalizedFromRankPatch), "GetExplanationForRank"));
                yield return new CodeInstruction(OpCodes.Stloc_0);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                patched = true;
            }
            yield return codeInstructions[i];
        }
    }

    public static StringBuilder GetExplanationForRank(StringBuilder stringBuilder, StatRequest req, StatDef stat)
    {
        if (stat == null)
        {
            return stringBuilder;
        }
        
        if (req.Thing is not Pawn pawn)
        {
            return stringBuilder;
        }

        if (!pawn.HasComp<CompRankInfo>())
        {
            return stringBuilder;
        }
            
        var rankListForReading = pawn.GetComp<CompRankInfo>().UnlockedRanks;
        var appendOverallRankText = true;
        foreach (var rank in rankListForReading)
        {
            if (rank == null)
            {
                continue;
            }
            var statOffsetFromRank = rank.statOffsets.GetStatOffsetFromList(stat);
            if (statOffsetFromRank != 0f)
            {
                if (appendOverallRankText)
                {
                    stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                    appendOverallRankText = false;
                }
                stringBuilder.AppendLine("    " + rank.LabelCap + ": " + Core40kUtils.ValueToString(stat, statOffsetFromRank, finalized: false, ToStringNumberSense.Offset));
            }
            var statFactorFromRank = rank.statFactors.GetStatFactorFromList(stat);
            if (statFactorFromRank != 1f)
            {
                if (appendOverallRankText)
                {
                    stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                    appendOverallRankText = false;
                }
                stringBuilder.AppendLine("    " + rank.LabelCap + ": " + Core40kUtils.ValueToString(stat, statFactorFromRank, finalized: false, ToStringNumberSense.Factor));
            }
            if (rank.conditionalStatAffecters.NullOrEmpty())
            {
                continue;
            }

            foreach (var conditionalStat in rank.conditionalStatAffecters)
            {
                if (!conditionalStat.Applies(req))
                {
                    continue;
                }
                var statOffsetFromRankConditional = conditionalStat.statOffsets.GetStatOffsetFromList(stat);
                if (statOffsetFromRankConditional != 0f)
                {
                    if (appendOverallRankText)
                    {
                        stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                        appendOverallRankText = false;
                    }
                    stringBuilder.AppendLine("    " + rank.LabelCap + " (" + conditionalStat.Label + "): " + Core40kUtils.ValueToString(stat, statOffsetFromRankConditional, finalized: false, ToStringNumberSense.Offset));
                }
                var statFactorFromRankConditional = conditionalStat.statFactors.GetStatFactorFromList(stat);
                if (statFactorFromRankConditional != 1f)
                {
                    if (appendOverallRankText)
                    {
                        stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                        appendOverallRankText = false;
                    }
                    stringBuilder.AppendLine("    " + rank.LabelCap + " (" + conditionalStat.Label + "): " + Core40kUtils.ValueToString(stat, statFactorFromRankConditional, finalized: false, ToStringNumberSense.Factor));
                }
            }
        }            

        return stringBuilder;
    }
}