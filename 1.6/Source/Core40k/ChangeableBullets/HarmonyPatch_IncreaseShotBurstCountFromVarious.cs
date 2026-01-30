using HarmonyLib;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(Verb), "BurstShotCount", MethodType.Getter)]
public static class IncreaseShotBurstCountFromVarious
{
    public static void Postfix(Verb_Shoot __instance, ref int __result)
    {
        var weaponDecoComp = __instance.EquipmentSource.GetComp<CompWeaponDecoration>();
        if (weaponDecoComp != null)
        {
            foreach (var weaponDecoration in weaponDecoComp.WeaponDecorations)
            {
                if (weaponDecoration.Key.verbModifier != null)
                {
                    __result += weaponDecoration.Key.verbModifier.additionalBurstShotCount;
                }
            }
        }
        var ammoChangerComp = __instance.EquipmentSource.GetComp<Comp_AmmoChanger>();
        if (ammoChangerComp != null)
        {
            __result += ammoChangerComp.ShotsPerBurst;
        }
    }
}