using RimWorld;
using Verse;

namespace Core40k;

[DefOf]
public static class Core40kDefOf
{
    public static DamageDef BEWH_WarpFlame;

    public static JobDef BEWH_OpenStylingStationDialogForApparelMultiColor;
    public static JobDef BEWH_OpenStylingStationDialogForWeaponMultiColor;
    public static JobDef BEWH_ChangeAmmo;

    public static ShaderTypeDef BEWH_CutoutThreeColor;

    static Core40kDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(Core40kDefOf));
    }
}