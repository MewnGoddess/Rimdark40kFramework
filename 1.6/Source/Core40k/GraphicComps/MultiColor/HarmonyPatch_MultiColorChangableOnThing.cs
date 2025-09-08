using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Core40k;

[HarmonyPatch(typeof(ThingWithComps), "GetFloatMenuOptions")]
public class MultiColorChangableOnThing
{
    public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, ThingWithComps __instance, Pawn selPawn)
    {
        foreach (var floatMenu in __result)
        {
            yield return floatMenu;
        }
        
        if (!__instance.def.HasModExtension<DefModExtension_AllowColoringOfThings>())
        {
            yield break;
        }
        //Apparel
        if (__instance.def.GetModExtension<DefModExtension_AllowColoringOfThings>().allowColoringOfApparel)
        {
            if (selPawn.apparel.WornApparel.Any(a => a.HasComp<CompMultiColor>()))
            {
                var secondColourChangeFloatMenu = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BEWH.Framework.ApparelMultiColor.ArmourDecorationFeature".Translate().CapitalizeFirst(), delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Core40kDefOf.BEWH_OpenStylingStationDialogForApparelMultiColor, __instance), JobTag.Misc);
                }), selPawn, __instance);
                yield return secondColourChangeFloatMenu;
            }
        }
        //Equipment
        if (__instance.def.GetModExtension<DefModExtension_AllowColoringOfThings>().allowColoringOfEquipment)
        {
            if (selPawn.equipment.Primary.HasComp<CompMultiColor>())
            {
                var changeFloatMenu = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BEWH.Framework.WeaponMultiColor.WeaponDecorationFeature".Translate().CapitalizeFirst(), delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Core40kDefOf.BEWH_OpenStylingStationDialogForWeaponMultiColor, __instance), JobTag.Misc);
                }), selPawn, __instance);
                yield return changeFloatMenu;
            }
        }
    }
}