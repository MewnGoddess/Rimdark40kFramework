using HarmonyLib;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(PawnRenderNode), "Props", MethodType.Getter)]
public static class ChangeDrawDataPatch
{
    public static void Postfix(ref PawnRenderNodeProperties __result, PawnRenderNode __instance)
    {
        var multiColor = __instance?.apparel?.GetComp<CompMultiColor>();
        if (multiColor?.currentAlternateBaseForm == null)
        {
            return;
        }
        
        __result.drawData = multiColor.currentAlternateBaseForm.drawData;
    }
}