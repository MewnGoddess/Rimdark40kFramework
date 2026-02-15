using System;
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
    
    [Obsolete]
    public List<CustomizationTabDef> tabDefs = [];
    
    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        if (!tabDefs.NullOrEmpty())
        {
            Log.Warning("tabDefs defined in CompProperties_MultiColor on " + parentDef.label + " should instead be defined in DefModExtension_AvailableDrawerTabDefs");
        }
    }
    
    public CompProperties_MultiColor()
    {
        compClass = typeof(CompMultiColor);
    }
}