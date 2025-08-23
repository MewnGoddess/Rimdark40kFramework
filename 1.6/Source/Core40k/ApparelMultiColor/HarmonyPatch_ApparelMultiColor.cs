using HarmonyLib;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
public static class ApparelMultiColorPatch
{
    public static bool Prefix(ref bool __result, Apparel apparel, BodyTypeDef bodyType, ref ApparelGraphicRecord rec)
    {
        if (apparel.WornGraphicPath.NullOrEmpty())
        {
            return true;
        }

        if (apparel is not ApparelMultiColor apparelMultiColor)
        {
            return true;
        }

        if (apparelMultiColor.RecacheGraphics)
        {
            var graphic = TryGetGraphicApparel(apparelMultiColor, bodyType);
            apparelMultiColor.CachedGraphicMulti = graphic;
        }

        rec = apparelMultiColor.ApparelGraphicRecord;
        __result = true;
        return false;
    }

    private static Graphic_Multi TryGetGraphicApparel(ApparelMultiColor apparel, BodyTypeDef bodyType)
    {
        if (bodyType == null)
        {
            Log.Error("Getting apparel graphic with undefined body type.");
            bodyType = BodyTypeDefOf.Male;
        }

        bodyType = apparel.def.GetModExtension<DefModExtension_ForcesBodyType>()?.forcedBodyType ?? bodyType;
        
        var path = ((apparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && apparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover && !apparel.RenderAsPack() && apparel.WornGraphicPath != BaseContent.PlaceholderImagePath && apparel.WornGraphicPath != BaseContent.PlaceholderGearImagePath) ? (apparel.WornGraphicPath + "_" + bodyType.defName) : apparel.WornGraphicPath);
        
        var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
        var maskPath = apparel.MaskDef?.maskPath;
        if (apparel.MaskDef != null && apparel.MaskDef.useBodyTypes)
        {
            maskPath += "_" + bodyType.defName;
        }
        var graphic = MultiColorUtils.GetGraphic<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor, apparel.DrawColorTwo, apparel.DrawColorThree, apparel.Graphic.data, maskPath);
        return graphic;
    }
}