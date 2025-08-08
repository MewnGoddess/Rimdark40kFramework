using HarmonyLib;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
public static class ApparelMultiColorPatch
{
    public static bool Prefix(ref bool __result, Apparel apparel, BodyTypeDef bodyType, ref ApparelGraphicRecord rec)
    {
        if (apparel is not ApparelMultiColor)
        {
            return true;
        }
            
        __result = TryGetGraphicApparel(apparel, bodyType, ref rec);
            
        return false;
    }

    private static bool TryGetGraphicApparel(Apparel apparel, BodyTypeDef bodyType, ref ApparelGraphicRecord rec)
    {
        if (bodyType == null)
        {
            Log.Error("Getting apparel graphic with undefined body type.");
            bodyType = BodyTypeDefOf.Male;
        }
        if (apparel.WornGraphicPath.NullOrEmpty())
        {
            rec = new ApparelGraphicRecord(null, null);
            return false;
        }

        bodyType = apparel.def.GetModExtension<DefModExtension_ForcesBodyType>()?.forcedBodyType ?? bodyType;
        
        var path = ((apparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && apparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover && !apparel.RenderAsPack() && apparel.WornGraphicPath != BaseContent.PlaceholderImagePath && apparel.WornGraphicPath != BaseContent.PlaceholderGearImagePath) ? (apparel.WornGraphicPath + "_" + bodyType.defName) : apparel.WornGraphicPath);

        var apparelMultiColor = (ApparelMultiColor)apparel;
        var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
        var graphic = MultiColorUtils.GetGraphic<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparelMultiColor.DrawColor, apparelMultiColor.DrawColorTwo, apparelMultiColor.DrawColorThree, apparelMultiColor.Graphic.data, apparelMultiColor.MaskDef?.maskPath);
        rec = new ApparelGraphicRecord(graphic, apparel);
        return true;
    }
}