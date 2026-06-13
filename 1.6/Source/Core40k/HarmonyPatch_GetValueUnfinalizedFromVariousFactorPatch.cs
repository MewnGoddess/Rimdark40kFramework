using HarmonyLib;
using RimWorld;
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

        if (CoreUtils.cachedDecoratives.TryGetValue(pawn, out var cachedDecoratives))
        {
            var stat = statWorker.stat;
        
            //Apparel factor
            if (!cachedDecoratives.apparels.NullOrEmpty())
            {
                foreach (var apparel in cachedDecoratives.apparels)
                {
                    var compApparel = apparel.GetComp<CompDecorative>();
                    num *= compApparel.GetStatFactor(stat);
                }
            }

            //Weapon factor
            if (cachedDecoratives.weapon != null)
            {
                var compWeapon = cachedDecoratives.weapon.GetComp<CompWeaponDecoration>();
                num *= compWeapon.GetStatFactor(stat);
            }
        }

        if (CoreUtils.cachedAlternateTexture.TryGetValue(pawn, out var cachedAlternateTexture))
        {
            var stat = statWorker.stat;
        
            //Apparel factor
            if (!cachedAlternateTexture.apparels.NullOrEmpty())
            {
                foreach (var apparel in cachedAlternateTexture.apparels)
                {
                    var compApparel = apparel.GetComp<CompAlternateTexture>();
                    num *= compApparel.GetStatFactor(stat);
                }
            }

            //Weapon factor
            if (cachedAlternateTexture.weapon != null)
            {
                var compWeapon = cachedAlternateTexture.weapon.GetComp<CompAlternateTexture>();
                num *= compWeapon.GetStatFactor(stat);
            }
        }
        
        return num;
    }
}