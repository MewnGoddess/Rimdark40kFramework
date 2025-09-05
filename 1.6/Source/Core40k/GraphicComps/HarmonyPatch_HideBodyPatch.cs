using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
[HarmonyPriority(Priority.LowerThanNormal)]
public static class HideBodyPatch
{
    public static void Postfix(Pawn pawn, ref Graphic __result)
    {
        if (!pawn.apparel.AnyApparel || !(pawn.RaceProps?.Humanlike).GetValueOrDefault())
        {
            return;
        }
        
        if (Enumerable.Any(pawn.apparel.WornApparel, apparel => apparel.def.HasModExtension<DefModExtension_ForcesBodyType>() && apparel.def.GetModExtension<DefModExtension_ForcesBodyType>().hideBodyGraphic))
        {
            __result = GraphicDatabase.Get<Graphic_Multi>("UI/EmptyImage", ShaderUtility.GetSkinShader(pawn), Vector2.one, pawn.story.SkinColor);
        }
    }
}