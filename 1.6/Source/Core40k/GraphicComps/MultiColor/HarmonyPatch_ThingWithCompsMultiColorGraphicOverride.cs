using HarmonyLib;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(Thing), "Graphic", MethodType.Getter)]
[HarmonyPriority(Priority.Normal)]
public class ThingWithCompsMultiColorGraphicOverride
{
    public static void Postfix(ref Graphic __result, Thing __instance)
    {
        if (!__instance.HasComp<CompMultiColor>())
        {
            return;
        }

        var multiColor = __instance.TryGetComp<CompMultiColor>();

        if (multiColor.RecacheSingleGraphics)
        {
            multiColor.SetSingleGraphic();
        }
        
        __result = multiColor.GetSingleGraphic();
    }
}