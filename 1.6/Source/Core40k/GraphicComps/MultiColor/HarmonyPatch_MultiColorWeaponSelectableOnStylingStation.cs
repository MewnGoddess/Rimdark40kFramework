using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Core40k;

[HarmonyPatch(typeof(ThingWithComps), "GetFloatMenuOptions")]
public class MultiColorWeaponSelectableOnStylingStation
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

        if (!__instance.def.GetModExtension<DefModExtension_AllowColoringOfThings>().allowColoringOfEquipment)
        {
            yield break;
        }

        if (!selPawn.equipment.Primary.HasComp<CompMultiColor>())
        {
            yield break;
        }
            
        var changeFloatMenu = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BEWH.Framework.WeaponMultiColor.WeaponDecorationFeature".Translate().CapitalizeFirst(), delegate
        {
            selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Core40kDefOf.BEWH_OpenStylingStationDialogForWeaponMultiColor, __instance), JobTag.Misc);
        }), selPawn, __instance);
        yield return changeFloatMenu;
    }
}