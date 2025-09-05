using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Core40k;

public class CompProperties_MultiColor : CompProperties
{
    public int colorMaskAmount = 3;

    public Color? defaultPrimaryColor = null;
    public Color? defaultSecondaryColor;
    public Color? defaultTertiaryColor;
    
    public List<ApparelMultiColorTabDef> tabDefs;
    
    public CompProperties_MultiColor()
    {
        compClass = typeof(CompMultiColor);
    }
}