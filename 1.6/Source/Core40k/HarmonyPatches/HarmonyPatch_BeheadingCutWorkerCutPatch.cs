using HarmonyLib;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(DamageWorker_Cut), "ChooseHitPart")]
public class BeheadingCutWorkerCutPatch
{
    public static void Postfix(ref BodyPartRecord __result, DamageInfo dinfo, Pawn pawn)
    {
        if (__result.def == BodyPartDefOf.Neck)
        {
            return;
        }

        var beheadingCut = dinfo.Weapon.GetModExtension<DefModExtension_BeheadingCut>();
        if (beheadingCut == null)
        {
            return;
        }

        if (Rand.Chance(beheadingCut.neckTargetingChance))
        {
            __result = pawn.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Neck);
        }
    }
}