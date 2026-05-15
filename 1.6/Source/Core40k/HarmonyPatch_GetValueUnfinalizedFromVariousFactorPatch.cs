using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
public static class GetValueUnfinalizedFromVariousFactorPatch
{
    private static GameComponent_CoreUtils coreUtils;
    private static GameComponent_CoreUtils CoreUtils => coreUtils ??= Current.Game.GetComponent<GameComponent_CoreUtils>();
    
    public static void Postfix(ref float __result, StatWorker __instance, StatRequest req)
    {
        if (req.Thing is not Pawn pawn)
        {
            return;
        }

        __result *= GetStatFactorForX(req, __instance, pawn);;
    }
    
    public static float GetStatFactorForX(StatRequest req, StatWorker statWorker, Pawn pawn)
    {
        var num = 1f;

        StatDef stat = null;
        
        //Rank factor
        var rankComp = pawn.GetComp<CompRankInfo>();
        if (rankComp != null && !rankComp.UnlockedRanks.NullOrEmpty())
        {
            stat = statWorker.stat;
            var rankListForReading = rankComp.UnlockedRanks;
            
            if (rankComp.CachedStatFactor.TryGetValue(stat, out var cachedStatFactor))
            {
                num *= cachedStatFactor;
            }
            else
            {
                var resNum = 1f;
                foreach (var rank in rankListForReading)
                {
                    if (rank == null)
                    {
                        continue;
                    }

                    if (!rank.statFactors.NullOrEmpty())
                    {
                        resNum *= rank.statFactors.GetStatFactorFromList(stat);
                    }
                }

                rankComp.CachedStatFactor.Add(stat, resNum);
                num *= resNum;
            }
            
            foreach (var rank in rankListForReading.Where(def => def.conditionalStatAffecters != null))
            {
                foreach (var conditional in rank.conditionalStatAffecters)
                {
                    if (!conditional.statFactors.NullOrEmpty() && conditional.Applies(req))
                    {
                        num *= conditional.statFactors.GetStatFactorFromList(stat);
                    }
                }
            }
        }

        if (!CoreUtils.cachedDecoratives.TryGetValue(pawn, out var cachedDecoratives))
        {
            return num;
        }

        if (stat == null)
        {
            stat = statWorker.stat;
        }
        
        //Apparel factor
        if (!cachedDecoratives.apparels.NullOrEmpty())
        {
            foreach (var apparel in cachedDecoratives.apparels)
            {
                var compApparel = apparel.GetComp<CompDecorative>();
                if (compApparel.CachedStatFactor.TryGetValue(stat, out var cachedStatFactor))
                {
                    num *= cachedStatFactor;
                }
                else
                {
                    var resNum = 1f;
                    
                    foreach (var extraDecoration in compApparel.ExtraDecorations)
                    {
                        if (!extraDecoration.Key.statFactors.NullOrEmpty())
                        {
                            resNum *= extraDecoration.Key.statFactors.GetStatFactorFromList(stat);
                        }
                    }
                    
                    compApparel.CachedStatFactor.Add(stat, resNum);
                    num *= resNum;
                }
                foreach (var extraDecoration in compApparel.ExtraDecorations)
                {
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
        if (cachedDecoratives.weapon != null)
        {
            var compWeapon = cachedDecoratives.weapon.GetComp<CompWeaponDecoration>();
            if (compWeapon.CachedStatFactor.TryGetValue(stat, out var cachedStatFactor))
            {
                num *= cachedStatFactor;
            }
            else
            {
                var resNum = 1f;
                    
                foreach (var weaponDecoration in compWeapon.WeaponDecorations)
                {
                    if (!weaponDecoration.Key.statFactors.NullOrEmpty())
                    {
                        resNum *= weaponDecoration.Key.statFactors.GetStatFactorFromList(stat);
                    }
                }
                    
                compWeapon.CachedStatFactor.Add(stat, resNum);
                num *= resNum;
            }
            
            foreach (var weaponDecoration in compWeapon.WeaponDecorations)
            {
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