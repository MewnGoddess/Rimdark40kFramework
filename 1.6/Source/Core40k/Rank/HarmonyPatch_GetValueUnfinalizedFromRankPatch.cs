using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
public static class GetValueUnfinalizedFromRankPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var addedOffset = false;
        var addedFactor = false;
        var codeInstructions = instructions.ToList();
        foreach (var instruction in codeInstructions)
        {
            if (!addedFactor && instruction.opcode == OpCodes.Ret)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetValueUnfinalizedFromRankPatch), "GetStatFactorForRank"));
                yield return new CodeInstruction(OpCodes.Stloc_0);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                addedFactor = true;
            }
                
            yield return instruction;
                
            if (!addedOffset && instruction.opcode == OpCodes.Stloc_0)
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetValueUnfinalizedFromRankPatch), "GetStatOffsetForRank"));
                yield return new CodeInstruction(OpCodes.Stloc_0);
                addedOffset = true;
            }
        }
    }

    public static float GetStatOffsetForRank(float num, StatRequest req, StatDef stat)
    {
        if (stat == null)
        {
            return num;
        }
        
        if (req.Thing is not Pawn pawn)
        {
            return num;
        }

        if (!pawn.HasComp<CompRankInfo>())
        {
            return num;
        }
            
        var rankListForReading = pawn.GetComp<CompRankInfo>().UnlockedRanks;
        
        foreach (var rank in rankListForReading)
        {
            num += rank.statOffsets.GetStatOffsetFromList(stat);
            foreach (var conditional in rank.conditionalStatAffecters)
            {
                if (!conditional.statOffsets.NullOrEmpty() && conditional.Applies(req))
                {
                    num += conditional.statOffsets.GetStatOffsetFromList(stat);
                }
            }
        }

        return num;
    }

    public static float GetStatFactorForRank(float num, StatRequest req, StatDef stat)
    {
        if (req.Thing is not Pawn pawn)
        {
            return num;
        }

        if (!pawn.HasComp<CompRankInfo>())
        {
            return num;
        }
            
        var rankListForReading = pawn.GetComp<CompRankInfo>().UnlockedRanks;
            
        foreach (var rank in rankListForReading)
        {
            num *= rank.statFactors.GetStatFactorFromList(stat);
            foreach (var conditional in rank.conditionalStatAffecters)
            {
                if (!conditional.statFactors.NullOrEmpty() && conditional.Applies(req))
                {
                    num *= conditional.statFactors.GetStatFactorFromList(stat);
                }
            }
        }
            
        return num;
    }
}