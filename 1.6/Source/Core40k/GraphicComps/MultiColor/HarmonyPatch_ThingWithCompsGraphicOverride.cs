using HarmonyLib;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(Thing), "Graphic", MethodType.Getter)]
public class ThingWithCompsGraphicOverride
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
        
        __result = __instance.TryGetComp<CompMultiColor>().Graphic;
    }
}