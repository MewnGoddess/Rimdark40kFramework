using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Core40k;

internal enum DecorationType
{
    Body,
    Head,
}

public class DynamicPawnRenderNodeSetup_DecorativeApparel : DynamicPawnRenderNodeSetup
{
    public override bool HumanlikeOnly => true;
    
    public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
    {
        var decorativeApparels = pawn.apparel.WornApparel.Where(apparel => apparel is DecorativeApparelColourTwo).Cast<DecorativeApparelColourTwo>();
        
        foreach (var decorativeApparel in decorativeApparels)
        {
            DecorationType type;
            
            switch (decorativeApparel)
            {
                case BodyDecorativeApparelColourTwo:
                    type = DecorationType.Body;
                    break;
                case HeadDecorativeApparelColourTwo:
                    type = DecorationType.Head;
                    break;
                default:
                    yield break;
            }
            
            foreach (var decoration in decorativeApparel.ExtraDecorations)
            {
                var pawnRenderNodeProperty = new PawnRenderNodeProperties
                {
                    nodeClass = typeof(PawnRenderNode_AttachmentExtraDecoration),
                    workerClass = typeof(PawnRenderNodeWorker_AttachmentExtraDecorationBody),
                    texPath = decoration.Key.drawnTextureIconPath,
                    shaderTypeDef = decoration.Key.shaderType,                                                                      
                    parentTagDef = PawnRenderNodeTagDefOf.Body,
                    drawData = decoration.Key.drawData,
                    drawSize = decoration.Key.drawSize,
                    flipGraphic = decoration.Value.Flipped,
                    color = decoration.Value.Color,
                };

                switch (type)
                {
                    case DecorationType.Body:
                        pawnRenderNodeProperty.parentTagDef = decoration.Key.drawInHeadSpace ? PawnRenderNodeTagDefOf.Head : PawnRenderNodeTagDefOf.Body;
                        pawnRenderNodeProperty.workerClass = typeof(PawnRenderNodeWorker_AttachmentExtraDecorationBody);
                        break;
                    case DecorationType.Head:
                        pawnRenderNodeProperty.parentTagDef = PawnRenderNodeTagDefOf.Head;
                        pawnRenderNodeProperty.workerClass = typeof(PawnRenderNodeWorker_AttachmentExtraDecorationHead);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                var pawnRenderNode = (PawnRenderNode_AttachmentExtraDecoration)Activator.CreateInstance(typeof(PawnRenderNode_AttachmentExtraDecoration), pawn, pawnRenderNodeProperty, tree);
                pawnRenderNode.ExtraDecorationDef = decoration.Key;

                yield return (pawnRenderNode, null);
            }
            
            yield break;
        }
    }
}