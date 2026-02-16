using Core40k;
using HarmonyLib;
using Verse;

namespace Genes40k;

[HarmonyPatch(typeof(Pawn), "SpawnSetup")]
[HarmonyPriority(401)]
public class SetupDefaultColorsForFactionPatch
{
    public static void Postfix(Pawn __instance)
    {
        if (__instance?.Faction == null || __instance.Faction.IsPlayer || !__instance.HasMultiColorThing())
        {
            return;
        }

        Core40kUtils.SetupColorsForPawn(__instance);
    }
}