using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Core40k;

public class DynamicPawnRenderNodeSetup_DecorativeApparelBody : DynamicPawnRenderNodeSetup
{
    public override bool HumanlikeOnly => true;
    
    public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
    {
        if (pawn.apparel == null || pawn.apparel.WornApparelCount == 0)
        {
            yield break;
        }
        
        var decorativeApparels = pawn.apparel.WornApparel.Where(apparel => apparel is BodyDecorativeApparelMultiColor).Cast<BodyDecorativeApparelMultiColor>();
        foreach (var decorativeApparel in decorativeApparels)
        {
            foreach (var decoration in decorativeApparel.ExtraDecorations)
            {
                var pawnRenderNodeProperty = new PawnRenderNodePropertiesMultiColor
                {
                    nodeClass = typeof(PawnRenderNode_AttachmentExtraDecoration),
                    texPath = decoration.Key.drawnTextureIconPath,
                    shaderTypeDef = decoration.Key.shaderType,                                                                      
                    drawData = decoration.Key.drawData,
                    drawSize = decoration.Key.drawSize,
                    flipGraphic = decoration.Value.Flipped,
                    color = decoration.Value.Color,
                    colorTwo = decoration.Value.ColorTwo,
                    colorThree = decoration.Value.ColorThree,
                    parentTagDef = decoration.Key.drawInHeadSpace ? PawnRenderNodeTagDefOf.Head : PawnRenderNodeTagDefOf.Body,
                    workerClass = typeof(PawnRenderNodeWorker_AttachmentExtraDecorationBody),
                };
                
                var pawnRenderNode = (PawnRenderNode_AttachmentExtraDecoration)Activator.CreateInstance(typeof(PawnRenderNode_AttachmentExtraDecoration), pawn, pawnRenderNodeProperty, tree);
                pawnRenderNode.ExtraDecorationDef = decoration.Key;

                PawnRenderNode node;
                if (decoration.Key.drawInHeadSpace)
                {
                    node = (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelHead, out node) ? node : null) ??
                                          (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.Head, out node) ? node : null);
                }
                else
                {
                    node = (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelBody, out node) ? node : null) ??
                           (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.Body, out node) ? node : null);
                }
                
                yield return (pawnRenderNode, node);
            }
            
            yield break;
        }
    }
}