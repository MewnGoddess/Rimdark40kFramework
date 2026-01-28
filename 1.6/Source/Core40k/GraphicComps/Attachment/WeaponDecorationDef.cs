using System.Collections.Generic;
using Verse;

namespace Core40k;

public class WeaponDecorationDef : DecorationDef
{
    public int layerPlacement = 1;

    public bool useWeaponColorAsDefault = true;

    public WeaponDecorationTypeDef decorationType;

    public Dictionary<string, DrawData> weaponSpecificDrawData = [];

    public bool isIncompatibleWithBaseTexture = false;
}