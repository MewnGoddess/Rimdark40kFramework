using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(Thing), "GetGizmos")]
public class GizmoTextureTogglePatch
{
    private static GameComponent_CoreUtils coreUtils;

    private static GameComponent_CoreUtils CoreUtils => coreUtils ??= Current.Game.GetComponent<GameComponent_CoreUtils>();
    
    public static IEnumerable<Gizmo> Postfix(Thing __instance)
    {
        var defMod = __instance.def.GetModExtension<DefModExtension_TextureFlags>();
        if (defMod == null)
        {
            yield break;
        }
        
        var Wearer = __instance.ParentHolder is not Pawn_ApparelTracker pawn_ApparelTracker ? null : pawn_ApparelTracker.pawn;
        var Holder = __instance.ParentHolder is not Pawn_EquipmentTracker pawn_EquipmentTracker ? null : pawn_EquipmentTracker.pawn;

        var pawn = Wearer ?? Holder;

        var pair = (pawn, __instance);
        
        if (pawn == null || !CoreUtils.cachedGizmoToggles.ContainsKey(pair))
        {
            yield break;
        }
        
        var gizmoOn = CoreUtils.cachedGizmoToggles[pair];
        
        foreach (var textureFlag in defMod.textureFlags.Where(flag => flag.gizmoActivated))
        {
            var toggleCommand = new Command_Toggle
            {
                defaultLabel = gizmoOn ? textureFlag.gizmoOnText : textureFlag.gizmoOffText,
                icon = Core40kUtils.FlippedIconTex,
                isActive = () => gizmoOn,
                toggleAction = delegate
                {
                    CoreUtils.cachedGizmoToggles[pair] = !CoreUtils.cachedGizmoToggles[pair];
                },
            };

            yield return toggleCommand;
        }
    }
}