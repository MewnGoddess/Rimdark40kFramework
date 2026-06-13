using HarmonyLib;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(StatWorker), "StatOffsetFromGear")]
public static class GetOffsetFromGearPatch
{
    private static GameComponent_CoreUtils coreUtils;

    private static GameComponent_CoreUtils CoreUtils => coreUtils ??= Current.Game.GetComponent<GameComponent_CoreUtils>();

    public static void Postfix(ref float __result, Thing gear, StatDef stat)
    {
        var decoComp = gear.TryGetComp<CompGraphicParent>();
        var colorComp = gear.TryGetComp<CompMultiColor>();
        if (decoComp != null)
        {
            __result += decoComp.GetStatOffset(stat);
        }
        if (colorComp != null)
        {
            __result += colorComp.GetStatOffset(stat);
        }
    }
}