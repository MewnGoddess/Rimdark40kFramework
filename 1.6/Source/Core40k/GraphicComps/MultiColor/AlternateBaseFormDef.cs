using System.Collections.Generic;

namespace Core40k;

public class AlternateBaseFormDef : DecorationDef
{
    public List<WeaponDecorationDef> incompatibleWeaponDecorations = [];
    public List<ExtraDecorationDef> incompatibleArmorDecorations = [];
    public List<MaskDef> incompatibleMaskDefs = [];
}