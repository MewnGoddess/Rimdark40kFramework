using RimWorld;
using Verse;

namespace Core40k;

[DefOf]
public static class Core40kDefOf
{
    public static DamageDef BEWH_WarpFlame;

    public static MaskDef BEWH_DefaultMask;
    
    [MayRequireRoyalty]
    public static ThingDef Apparel_PackJump;

    public static JobDef BEWH_OpenStylingStationDialogForApparelMultiColor;
    public static JobDef BEWH_OpenStylingStationDialogForWeaponMultiColor;
    public static JobDef BEWH_ChangeAmmo;

    public static ShaderTypeDef BEWH_CutoutThreeColor;
    
    public static StatDef BEWH_ArtificialPartsAffinityFactor;
    public static StatDef BEWH_RankLearningFactor;
    
    public static JoyKindDef BEWH_RecreationFromSkill;

    static Core40kDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(Core40kDefOf));
    }
}