using HarmonyLib;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(Thing), "Graphic", MethodType.Getter)]
[HarmonyPriority(Priority.Normal)]
public class ThingWithCompsMultiColorGraphicOverride
{
    public static void Postfix(ref Graphic __result, Thing __instance)
    {
        if (__instance is not ThingWithComps weapon)
        {
            return;
        }
        
        if (!__instance.HasComp<CompMultiColor>() && !__instance.HasComp<CompAlternateTexture>())
        {
            return;
        }
        
        var multiColor = weapon.GetComp<CompMultiColor>();
        var alternateTexture = weapon.GetComp<CompAlternateTexture>();

        if ((multiColor != null && multiColor.RecacheSingleGraphics) || (alternateTexture != null && alternateTexture.RecacheSingleGraphics))
        {
            multiColor?.SetSingleGraphic();
            alternateTexture?.SetSingleGraphic();
        }

        if (multiColor != null)
        {
            __result = multiColor.GetSingleGraphic();
        }
        else if (alternateTexture != null)
        {
            __result = alternateTexture.GetSingleGraphic();
        }
    }
}