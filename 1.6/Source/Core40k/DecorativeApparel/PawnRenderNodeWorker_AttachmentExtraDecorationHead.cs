using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class PawnRenderNodeWorker_AttachmentExtraDecorationHead : PawnRenderNodeWorker
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        var pawn = parms.pawn;
            
        var apparelMultiColor = (HeadDecorativeApparelMultiColor)pawn.apparel.WornApparel.FirstOrDefault(wornApparel => wornApparel is HeadDecorativeApparelMultiColor);

        var decoration = apparelMultiColor.ExtraDecorations.Keys.FirstOrFallback(def => def.drawnTextureIconPath == node.Props.texPath);

        if (decoration == null)
        {
            return false;
        }
            
        var showWhenFacing = new List<Rot4>();
        if (node.Props.flipGraphic)
        {
            showWhenFacing.AddRange(decoration.defaultShowRotation.Select(rotation => rotation.Opposite));
        } 
        else
        {
            showWhenFacing = decoration.defaultShowRotation;
        }
        if (parms.Portrait)
        {
            if (!showWhenFacing.Contains(parms.facing))
            {
                return false;
            }

            if ((parms.flags & PawnRenderFlags.Headgear) != PawnRenderFlags.Headgear)
            {
                return false;
            }
            
            if ((parms.flags & PawnRenderFlags.Clothes) != PawnRenderFlags.Clothes)
            {
                return false;
            }
        }
        else
        {
            if (parms.posture is PawnPosture.LayingOnGroundNormal or PawnPosture.LayingOnGroundFaceUp)
            {
                return true;
            }
                
            if (!showWhenFacing.Contains(parms.facing))
            {
                return false;
            }
                
            if (parms.posture == PawnPosture.Standing)
            {
                return true;
            }
            
            var mindState = pawn.mindState;
            if (mindState != null && mindState.duty?.def?.drawBodyOverride.HasValue == true)
            {
                return pawn.mindState.duty.def.drawBodyOverride.Value;
            }
            if (parms.bed != null && parms.pawn.RaceProps.Humanlike)
            {
                return parms.bed.def.building.bed_showSleeperBody;
            }
        }
            
        return base.CanDrawNow(node, parms);
    }
}