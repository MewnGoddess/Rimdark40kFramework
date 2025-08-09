using UnityEngine;
using Verse;

namespace Core40k;

public class DefModExtension_WeaponMultiColor : DefModExtension
{
    public int colorMaskAmount = 1;

    public Color? defaultPrimaryColor = null;
    public Color? defaultSecondaryColor;
    public Color? defaultTertiaryColor;
}