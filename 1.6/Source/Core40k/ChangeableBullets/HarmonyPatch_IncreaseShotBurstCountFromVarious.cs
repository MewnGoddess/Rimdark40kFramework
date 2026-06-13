using HarmonyLib;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(Verb), "BurstShotCount", MethodType.Getter)]
public static class IncreaseShotBurstCountFromVarious
{
    public static void Postfix(Verb __instance, ref int __result)
    {
        var ammoChangerComp = __instance?.EquipmentSource?.GetComp<Comp_AmmoChanger>();
        if (ammoChangerComp != null)
        {
            __result = ammoChangerComp.ShotsPerBurst;
        }
        var weaponDecoComp = __instance?.EquipmentSource?.GetComp<CompWeaponDecoration>();
        if (weaponDecoComp?.Decorations != null)
        {
            foreach (var weaponDecoration in weaponDecoComp.Decorations)
            {
                if (weaponDecoration.Key is not WeaponDecorationDef weaponDecorationDef)
                {
                    continue;
                }
                
                if (weaponDecorationDef.verbModifier != null)
                {
                    __result += weaponDecorationDef.verbModifier.additionalBurstShotCount;
                }
            }
        }
    }
}