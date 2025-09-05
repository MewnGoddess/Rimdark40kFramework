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

        if (!apparel.HasComp<CompMultiColor>())
        {
            return true;
        }
        
        var multiColor = apparel.GetComp<CompMultiColor>();

        if (multiColor.RecacheGraphics)
        {
            var graphic = TryGetGraphicApparel(apparel, multiColor, bodyType);
            multiColor.CachedGraphicMulti = graphic;
        }

        rec = multiColor.ApparelGraphicRecord;
        __result = true;
        return false;
    }

    private static Graphic_Multi TryGetGraphicApparel(Apparel apparel, CompMultiColor multiColor, BodyTypeDef bodyType)
    {
        if (bodyType == null)
        {
            Log.Error("Getting apparel graphic with undefined body type.");
            bodyType = BodyTypeDefOf.Male;
        }

        bodyType = apparel.def.GetModExtension<DefModExtension_ForcesBodyType>()?.forcedBodyType ?? bodyType;
        
        var path = ((apparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && apparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover && !apparel.RenderAsPack() && apparel.WornGraphicPath != BaseContent.PlaceholderImagePath && apparel.WornGraphicPath != BaseContent.PlaceholderGearImagePath) ? (apparel.WornGraphicPath + "_" + bodyType.defName) : apparel.WornGraphicPath);
        
        var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
        var maskPath = multiColor.MaskDef?.maskPath;
        if (multiColor.MaskDef != null && multiColor.MaskDef.useBodyTypes)
        {
            maskPath += "_" + bodyType.defName;
        }
        var graphic = MultiColorUtils.GetGraphic<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, multiColor.DrawColor, multiColor.DrawColorTwo, multiColor.DrawColorThree, null, maskPath);
        return graphic;
    }
}