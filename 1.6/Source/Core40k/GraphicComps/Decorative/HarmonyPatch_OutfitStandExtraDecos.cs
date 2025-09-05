// using System.Collections.Generic;
// using System.Linq;
// using HarmonyLib;
// using RimWorld;
// using UnityEngine;
// using Verse;
//
// namespace Core40k;
//
// [HarmonyPatch(typeof(Building_OutfitStand), "RecacheGraphics")]
// public static class OutfitStandExtraDecos
// {
//     public static void Postfix(Building_OutfitStand __instance, ThingOwner<Thing> ___innerContainer, List<CachedGraphicRenderInfo> ___cachedApparelGraphicsNonHeadgear/*, List<CachedGraphicRenderInfo> ___cachedApparelGraphicsHeadgear, bool ___cachedApparelRenderInfoSkipHead*/)
//     {
//         var list = ___innerContainer.InnerListForReading.OfType<Apparel>().ToList();
//         foreach (var item in list)
//         {
//             if (item is not DecorativeApparelMultiColor decorativeApparel)
//             {
//                 continue;
//             }
//
//             foreach (var extraDecoration in decorativeApparel.ExtraDecorations)
//             {
//                 Log.Message("extraDecoration.Key: " + extraDecoration.Key);
//                 Log.Message("extraDecoration.Value: " + extraDecoration.Value);
//                 var texPath = extraDecoration.Key.drawnTextureIconPath;
//                 var maskPath = extraDecoration.Key.useMask ? texPath + "_mask" : null;
//                 var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
//                 var graphic = MultiColorUtils.GetGraphic<Graphic_Multi>(texPath, shader, extraDecoration.Key.drawSize, extraDecoration.Value.Color, extraDecoration.Value.ColorTwo, extraDecoration.Value.ColorThree, decorativeApparel.Graphic?.data, maskPath);
//                 var layer = (int)extraDecoration.Key.drawData.LayerForRot(__instance.Rotation, 70);
//                 var vector = Vector3.zero;
//                 
//                 Log.Message("texPath: " + texPath);
//                 Log.Message("maskPath: " + maskPath);
//                 Log.Message("shader: " + shader);
//                 Log.Message("graphic: " + graphic);
//                 Log.Message("layer: " + layer);
//                 Log.Message("vector: " + vector);
//                 Log.Message("extraDecoration.Key.drawSize: " + extraDecoration.Key.drawSize);
//                 Log.Message("extraDecoration.Key.drawData: " + extraDecoration.Key.drawData);
//                 Log.Message("decorativeApparel.Graphic?.data: " + decorativeApparel.Graphic?.data);
//                 
//                 var cachedGraphicRenderInfo = new CachedGraphicRenderInfo(graphic, layer, extraDecoration.Key.drawSize, vector);
//                 Log.Message("cachedGraphicRenderInfo: " + cachedGraphicRenderInfo);
//                 Log.Message("___cachedApparelGraphicsNonHeadgear: " + ___cachedApparelGraphicsNonHeadgear);
//                 ___cachedApparelGraphicsNonHeadgear.Add(cachedGraphicRenderInfo);
//                 /*if (extraDecoration.Key.drawInHeadSpace || extraDecoration.Key.isHelmetDecoration)
//                 {
//                     vector.y = BodyTypeDefOf.Hulk.headOffset.y;
//                     var cachedGraphicRenderInfo = new CachedGraphicRenderInfo(graphic, layer, extraDecoration.Key.drawSize, vector);
//                     ___cachedApparelGraphicsHeadgear.Add(cachedGraphicRenderInfo);
//                     ___cachedApparelRenderInfoSkipHead = false;
//                 }
//                 else
//                 {
//                     var cachedGraphicRenderInfo = new CachedGraphicRenderInfo(graphic, layer, extraDecoration.Key.drawSize, vector);
//                     ___cachedApparelGraphicsNonHeadgear.Add(cachedGraphicRenderInfo);
//                 }*/
//             }
//         }
//     }
// }