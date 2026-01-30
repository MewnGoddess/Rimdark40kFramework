using System.Collections.Generic;
using System.Text;
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

    public override bool HasRequirements(Pawn pawn, out string lockedReason)
    {
        var requirementFulfilled = base.HasRequirements(pawn, out lockedReason);
        if (appliesToBodyTypes.NullOrEmpty())
        {
            return requirementFulfilled;
        }
        var bodyApparel = pawn.apparel.WornApparel.FirstOrFallback(a => a.HasComp<CompDecorative>());
        if (bodyApparel == null)
        {
            return requirementFulfilled;
        }
        
        var reason = new StringBuilder();
        reason.AppendLine(lockedReason);
        
        var pawnBodyType = bodyApparel.def?.GetModExtension<DefModExtension_ForcesBodyType>()?.forcedBodyType ?? pawn.story.bodyType;
        if (!appliesToBodyTypes.Contains(pawnBodyType))
        {
            reason.AppendLine("BEWH.Framework.DecoRequirement.InvalidBodytype".Translate());
            lockedReason = reason.ToString();
            requirementFulfilled = false;
        }
        
        return requirementFulfilled;
    }
    
    public override void ResolveReferences()
    {
        base.ResolveReferences();
        defaultMask ??= Core40kDefOf.BEWH_DefaultMask;
    }
}