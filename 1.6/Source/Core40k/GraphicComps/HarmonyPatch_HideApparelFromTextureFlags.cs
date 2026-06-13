using HarmonyLib;
using RimWorld;

namespace Core40k;

[HarmonyPatch(typeof(Apparel), "WornGraphicPath", MethodType.Getter)]
public class HideApparelFromTextureFlags
{
    public static void Postfix(ref string __result, Apparel __instance)
    {
        if (__instance.Wearer?.apparel?.WornApparel == null)
        {
            return;
        }

        var texPath = __result;
        
        foreach (var apparel in __instance.Wearer.apparel.WornApparel)
        {
            var defMod = apparel.def.GetModExtension<DefModExtension_TextureFlags>();
            if (defMod == null)
            {
                continue;
            }

            foreach (var flag in defMod.textureFlags)
            {
                if (flag.thingActivator == __instance.def && flag.hideThing)
                {
                    texPath = "Things/Armor/Imperium/PowerArmor/CommonIcons/BEWH_None";
                }
            }
        }

        __result = texPath;
    }
}