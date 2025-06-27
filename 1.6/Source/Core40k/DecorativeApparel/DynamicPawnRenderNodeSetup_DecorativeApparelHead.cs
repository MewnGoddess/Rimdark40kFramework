using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Core40k;

public class DynamicPawnRenderNodeSetup_DecorativeApparelHead : DynamicPawnRenderNodeSetup
{
    public override bool HumanlikeOnly => true;
    
    public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
    {
        if (pawn.apparel == null || pawn.apparel.WornApparelCount == 0)
        {
            yield break;
        }
        
        var decorativeApparels = pawn.apparel.WornApparel.Where(apparel => apparel is HeadDecorativeApparelColourTwo).Cast<HeadDecorativeApparelColourTwo>();
        foreach (var decorativeApparel in decorativeApparels)
        {
            foreach (var decoration in ((DecorativeApparelColourTwo)decorativeApparel).ExtraDecorations)
            {
                var pawnRenderNodeProperty = new PawnRenderNodeProperties
                {
                    nodeClass = typeof(PawnRenderNode_AttachmentExtraDecoration),
                    texPath = decoration.Key.drawnTextureIconPath,
                    shaderTypeDef = decoration.Key.shaderType,                                                                      
                    drawData = decoration.Key.drawData,
                    drawSize = decoration.Key.drawSize,
                    flipGraphic = decoration.Value.Flipped,
                    color = decoration.Value.Color,
                    parentTagDef = PawnRenderNodeTagDefOf.Head,
                    workerClass = typeof(PawnRenderNodeWorker_AttachmentExtraDecorationHead),
                };
                
                var pawnRenderNode = (PawnRenderNode_AttachmentExtraDecoration)Activator.CreateInstance(typeof(PawnRenderNode_AttachmentExtraDecoration), pawn, pawnRenderNodeProperty, tree);
                pawnRenderNode.ExtraDecorationDef = decoration.Key;

                PawnRenderNode node = (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelHead, out node) ? node : null) ??
                                      (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.Head, out node) ? node : null);

                yield return (pawnRenderNode, node);
            }
            
            yield break;
        }
    }
}