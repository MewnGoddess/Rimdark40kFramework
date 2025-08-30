using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Core40k;

[HarmonyPatch(typeof(ThingWithComps), "GetFloatMenuOptions")]
public class MultiColorApparelSelectableOnStylingStation
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
        
        if (!__instance.def.GetModExtension<DefModExtension_AllowColoringOfThings>().allowColoringOfApparel)
        {
            yield break;
        }

        if (!selPawn.apparel.WornApparel.Any(a => a.HasComp<CompMultiColor>()))
        {
            yield break;
        }

        var secondColourChangeFloatMenu = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BEWH.Framework.ApparelMultiColor.ArmourDecorationFeature".Translate().CapitalizeFirst(), delegate
        {
            selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Core40kDefOf.BEWH_OpenStylingStationDialogForApparelMultiColor, __instance), JobTag.Misc);
        }), selPawn, __instance);
        yield return secondColourChangeFloatMenu;
    }
}