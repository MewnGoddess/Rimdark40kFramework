using System.Collections.Generic;
using Verse;

namespace Core40k;

public class WeaponDecorationDef : DecorationDef
{
    public float layerPlacement = 1f;

    public Dictionary<string, DrawData> weaponSpecificDrawData = [];
    
    public VerbModifier verbModifier = null;
}