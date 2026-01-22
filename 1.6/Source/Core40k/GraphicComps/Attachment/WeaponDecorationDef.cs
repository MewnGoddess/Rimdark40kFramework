using UnityEngine;

namespace Core40k;

public class WeaponDecorationDef : DecorationDef
{
    public int layerPlacement = 1;

    public bool useWeaponColorAsDefault = true;

    public WeaponDecorationTypeDef decorationType;

    public bool isIncompatibleWithBaseTexture = false;
}