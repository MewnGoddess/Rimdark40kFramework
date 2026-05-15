using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
public static class GetValueUnfinalizedFromVariousOffsetPatch
{
    private static GameComponent_CoreUtils coreUtils;

    private static GameComponent_CoreUtils CoreUtils =>
        coreUtils ??= Current.Game.GetComponent<GameComponent_CoreUtils>();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var addedOffset = false;
        foreach (var instruction in instructions)
        {
            if (!addedOffset && instruction.opcode == OpCodes.Stloc_0)
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetValueUnfinalizedFromVariousOffsetPatch), "GetStatOffsetForX"));
                yield return new CodeInstruction(OpCodes.Stloc_0);
                addedOffset = true;
            }

            yield return instruction;
        }
    }

    public static float GetStatOffsetForX(float num, StatRequest req, StatDef stat)
    {
        if (req.Thing is not Pawn pawn)
        {
            return num;
        }

        //Rank offset
        var rankComp = pawn.GetComp<CompRankInfo>();
        if (rankComp != null && !rankComp.UnlockedRanks.NullOrEmpty())
        {
            var rankListForReading = rankComp.UnlockedRanks;

            if (rankComp.CachedStatOffset.TryGetValue(stat, out var cachedStatOffset))
            {
                num += cachedStatOffset;
            }
            else
            {
                var resNum = 0f;
                foreach (var rank in rankListForReading)
                {
                    if (rank == null)
                    {
                        continue;
                    }

                    if (!rank.statOffsets.NullOrEmpty())
                    {
                        resNum += rank.statOffsets.GetStatOffsetFromList(stat);
                    }
                }

                rankComp.CachedStatOffset.Add(stat, resNum);
                num += resNum;
            }

            foreach (var rank in rankListForReading.Where(def => def.conditionalStatAffecters != null))
            {
                foreach (var conditional in rank.conditionalStatAffecters)
                {
                    if (!conditional.statOffsets.NullOrEmpty() && conditional.Applies(req))
                    {
                        num += conditional.statOffsets.GetStatOffsetFromList(stat);
                    }
                }
            }
        }

        if (!CoreUtils.cachedDecoratives.TryGetValue(pawn, out var cachedDecoratives))
        {
            return num;
        }

        //Apparel offset
        if (!cachedDecoratives.apparels.NullOrEmpty())
        {
            foreach (var apparel in cachedDecoratives.apparels)
            {
                var compApparel = apparel.GetComp<CompDecorative>();
                if (compApparel.CachedStatOffset.TryGetValue(stat, out var cachedStatOffset))
                {
                    num += cachedStatOffset;
                }
                else
                {
                    var resNum = 0f;

                    foreach (var extraDecoration in compApparel.ExtraDecorations)
                    {
                        if (!extraDecoration.Key.statOffsets.NullOrEmpty())
                        {
                            resNum += extraDecoration.Key.statOffsets.GetStatOffsetFromList(stat);
                        }
                    }

                    compApparel.CachedStatOffset.Add(stat, resNum);
                    num += resNum;
                }

                foreach (var extraDecoration in compApparel.ExtraDecorations)
                {
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
        if (cachedDecoratives.weapon != null)
        {
            var compWeapon = cachedDecoratives.weapon.GetComp<CompWeaponDecoration>();
            if (compWeapon.CachedStatOffset.TryGetValue(stat, out var cachedStatOffset))
            {
                num += cachedStatOffset;
            }
            else
            {
                var resNum = 0f;

                foreach (var weaponDecoration in compWeapon.WeaponDecorations)
                {
                    if (!weaponDecoration.Key.statOffsets.NullOrEmpty())
                    {
                        resNum += weaponDecoration.Key.statOffsets.GetStatOffsetFromList(stat);
                    }
                }

                compWeapon.CachedStatOffset.Add(stat, resNum);
                num += resNum;
            }

            foreach (var weaponDecoration in compWeapon.WeaponDecorations)
            {
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
}