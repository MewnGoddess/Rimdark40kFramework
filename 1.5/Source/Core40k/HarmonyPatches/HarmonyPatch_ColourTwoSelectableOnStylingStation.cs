using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Core40k
{
    [HarmonyPatch(typeof(Building_StylingStation), "GetFloatMenuOptions")]
    public class ColourTwoSelectableOnStylingStation
    {
        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, Building_StylingStation __instance, Pawn selPawn)
        {
            foreach (var floatMenu in __result)
            {
                yield return floatMenu;
            }

            if (!selPawn.apparel.WornApparel.Any(a => a is ApparelColourTwo))
            {
                yield break;
            }
            
            var secondColourChangeFloatMenu = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BEWH.ApparelColourTwo.ChangeSecondaryColour".Translate().CapitalizeFirst(), delegate
            {
                selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Core40kDefOf.BEWH_OpenStylingStationDialogForSecondColour, __instance), JobTag.Misc);
            }), selPawn, __instance);
            yield return secondColourChangeFloatMenu;
        }
    }
}