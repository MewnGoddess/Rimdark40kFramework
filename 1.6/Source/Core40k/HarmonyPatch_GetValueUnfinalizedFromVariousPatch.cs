using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
public static class GetValueUnfinalizedFromVariousPatch
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
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetValueUnfinalizedFromVariousPatch), "GetStatOffsetForX"));
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
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetValueUnfinalizedFromVariousPatch), "GetStatFactorForX"));
                yield return new CodeInstruction(OpCodes.Stloc_0);
                addedOffset = true;
            }
        }
    }

    public static float GetStatOffsetForX(float num, StatRequest req, StatDef stat)
    {
        if (stat == null)
        {
            return num;
        }
        if (req.Thing is not Pawn pawn)
        {
            return num;
        }

        //Rank offset
        if (pawn.HasComp<CompRankInfo>())
        {
            var rankListForReading = pawn.GetComp<CompRankInfo>().UnlockedRanks;
        
            if (!rankListForReading.NullOrEmpty())
            {
                foreach (var rank in rankListForReading)
                {
                    if (rank == null || rank.statOffsets.NullOrEmpty())
                    {
                        continue;
                    }
                    num += rank.statOffsets.GetStatOffsetFromList(stat);
                    foreach (var conditional in rank.conditionalStatAffecters)
                    {
                        if (!conditional.statOffsets.NullOrEmpty() && conditional.Applies(req))
                        {
                            num += conditional.statOffsets.GetStatOffsetFromList(stat);
                        }
                    }
                }
            }
        }
        
        //Apparel offset
        var apparels = pawn.apparel?.WornApparel?.Where(apparel => apparel.HasComp<CompDecorative>()).ToList();
        if (apparels != null)
        {
            foreach (var apparel in apparels)
            {
                var compApparel = apparel.TryGetComp<CompDecorative>();
                if (compApparel == null)
                {
                    continue;
                }

                foreach (var extraDecoration in compApparel.ExtraDecorations)
                {
                    if (!extraDecoration.Key.statOffsets.NullOrEmpty())
                    {
                        num += extraDecoration.Key.statOffsets.GetStatOffsetFromList(stat);
                    }
                
                    foreach (var conditional in extraDecoration.Key.conditionalStatAffecters)
                    {
                        if (!conditional.statOffsets.NullOrEmpty() && conditional.Applies(req))
                        {
                            num += conditional.statOffsets.GetStatOffsetFromList(stat);
                        }
                    }
                }
            }
        }
        
        //Weapon offset
        var weapon = pawn.equipment?.Primary;
        var compWeapon = weapon?.TryGetComp<CompWeaponDecoration>();
        if (compWeapon != null)
        {
            foreach (var weaponDecoration in compWeapon.WeaponDecorations)
            {
                if (!weaponDecoration.Key.statOffsets.NullOrEmpty())
                {
                    num += weaponDecoration.Key.statOffsets.GetStatOffsetFromList(stat);
                }
                
                foreach (var conditional in weaponDecoration.Key.conditionalStatAffecters)
                {
                    if (!conditional.statOffsets.NullOrEmpty() && conditional.Applies(req))
                    {
                        num += conditional.statOffsets.GetStatOffsetFromList(stat);
                    }
                }
            }
        }

        return num;
    }
    
    public static float GetStatFactorForX(float num, StatRequest req, StatDef stat)
    {
        if (stat == null)
        {
            return num;
        }
        if (req.Thing is not Pawn pawn)
        {
            return num;
        }
        
        //Rank factor
        if (pawn.HasComp<CompRankInfo>())
        {
            var rankListForReading = pawn.GetComp<CompRankInfo>().UnlockedRanks;
        
            if (!rankListForReading.NullOrEmpty())
            {
                foreach (var rank in rankListForReading)
                {
                    if (rank == null || rank.statFactors.NullOrEmpty())
                    {
                        continue;
                    }
                    num *= rank.statFactors.GetStatFactorFromList(stat);
                    foreach (var conditional in rank.conditionalStatAffecters)
                    {
                        if (!conditional.statFactors.NullOrEmpty() && conditional.Applies(req))
                        {
                            num *= conditional.statFactors.GetStatFactorFromList(stat);
                        }
                    }
                }
            }
        }
        
        //Apparel factor
        var apparels = pawn.apparel?.WornApparel?.Where(apparel => apparel.HasComp<CompDecorative>()).ToList();
        if (apparels != null)
        {
            foreach (var apparel in apparels)
            {
                var compApparel = apparel.TryGetComp<CompDecorative>();
                if (compApparel == null)
                {
                    continue;
                }

                foreach (var extraDecoration in compApparel.ExtraDecorations)
                {
                    if (!extraDecoration.Key.statFactors.NullOrEmpty())
                    {
                        num *= extraDecoration.Key.statFactors.GetStatFactorFromList(stat);
                    }
                
                    foreach (var conditional in extraDecoration.Key.conditionalStatAffecters)
                    {
                        if (!conditional.statFactors.NullOrEmpty() && conditional.Applies(req))
                        {
                            num *= conditional.statFactors.GetStatFactorFromList(stat);
                        }
                    }
                }
            }
        }
        
        //Weapon factor
        var weapon = pawn.equipment?.Primary;
        var compWeapon = weapon?.TryGetComp<CompWeaponDecoration>();
        if (compWeapon != null)
        {
            foreach (var weaponDecoration in compWeapon.WeaponDecorations)
            {
                if (!weaponDecoration.Key.statFactors.NullOrEmpty())
                {
                    num *= weaponDecoration.Key.statFactors.GetStatFactorFromList(stat);
                }
                
                foreach (var conditional in weaponDecoration.Key.conditionalStatAffecters)
                {
                    if (!conditional.statFactors.NullOrEmpty() && conditional.Applies(req))
                    {
                        num *= conditional.statFactors.GetStatFactorFromList(stat);
                    }
                }
            }
        }

        return num;
    }
}