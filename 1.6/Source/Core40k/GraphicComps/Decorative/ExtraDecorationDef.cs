using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class ExtraDecorationDef : DecorationDef
{
    public bool isHelmetDecoration = false;
        
    public List<Rot4> defaultShowRotation = new() {Rot4.North, Rot4.South, Rot4.East, Rot4.West};
        
    public bool flipable = false;

    public bool hasArmorColourPaletteOption = false;
        
    public bool useArmorColourAsDefault = false;
        
    public bool drawInHeadSpace = false;
    
    public List<BodyTypeDef> appliesToBodyTypes = new();

    public override bool HasRequirements(Pawn pawn)
    {
        if (appliesToBodyTypes.NullOrEmpty())
        {
            return base.HasRequirements(pawn);
        }
        var bodyApparel = pawn.apparel.WornApparel.FirstOrFallback(a => a.HasComp<CompDecorative>());
        if (bodyApparel == null)
        {
            return base.HasRequirements(pawn);
        }

        var pawnBodyType = pawn.story.bodyType;
        var defMod = bodyApparel.def.GetModExtension<DefModExtension_ForcesBodyType>();
        if (defMod != null)
        {
            pawnBodyType = defMod.forcedBodyType ?? pawnBodyType;
        }

        return appliesToBodyTypes.Contains(pawnBodyType);
    }
    
    public override void ResolveReferences()
    {
        base.ResolveReferences();
        defaultMask ??= Core40kDefOf.BEWH_DefaultMask;
    }
}