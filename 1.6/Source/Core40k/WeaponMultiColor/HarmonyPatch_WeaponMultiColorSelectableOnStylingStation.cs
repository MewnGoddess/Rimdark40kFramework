using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Core40k;

[HarmonyPatch(typeof(Building_StylingStation), "GetFloatMenuOptions")]
public class WeaponMultiColorSelectableOnStylingStation
{
    public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, Building_StylingStation __instance, Pawn selPawn)
    {
        foreach (var floatMenu in __result)
        {
            yield return floatMenu;
        }
        
        if (selPawn.equipment.Primary is not WeaponMultiColor)
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