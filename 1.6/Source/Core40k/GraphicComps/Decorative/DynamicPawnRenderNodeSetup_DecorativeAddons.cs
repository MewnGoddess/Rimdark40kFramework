using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Core40k;

public class DynamicPawnRenderNodeSetup_DecorativeAddons : DynamicPawnRenderNodeSetup
{
    public override bool HumanlikeOnly => true;
    
    public override IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> GetDynamicNodes(Pawn pawn, PawnRenderTree tree)
    {
        if (pawn.apparel == null || pawn.apparel.WornApparelCount == 0)
        {
            yield break;
        }

        var decorativeApparels = pawn.apparel.WornApparel.Where(apparel => apparel.HasComp<CompDecorative>()).ToList();
        
        var pawnBodyType = pawn.story.bodyType;
        
        foreach (var decorativeApparel in decorativeApparels)
        {
            var decorativeComp = decorativeApparel.GetComp<CompDecorative>();
            
            if (decorativeApparel.def.HasModExtension<DefModExtension_ForcesBodyType>())
            {
                pawnBodyType = decorativeApparel.def.GetModExtension<DefModExtension_ForcesBodyType>().forcedBodyType ?? pawnBodyType;
            }
            
            foreach (var decoration in decorativeComp.ExtraDecorations)
            {
                PawnRenderNodePropertiesMultiColor pawnRenderNodeProperty;
                PawnRenderNode node;
                switch (decorativeComp.Props.decorativeType)
                {
                    case DecorativeType.Head:
                        pawnRenderNodeProperty = MakeHeadProps(decoration);
                        node = (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelHead, out node) ? node : null) ??
                                              (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.Head, out node) ? node : null);
                        break;
                    case DecorativeType.Body:
                        pawnRenderNodeProperty = MakeBodyProps(decoration,pawnBodyType);
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
                        break;
                    default:
                        continue;
                }
                
                var pawnRenderNode = (PawnRenderNode_AttachmentExtraDecoration)Activator.CreateInstance(typeof(PawnRenderNode_AttachmentExtraDecoration), pawn, pawnRenderNodeProperty, tree);
                
                yield return (pawnRenderNode, node);
            }
        }
    }

    private PawnRenderNodePropertiesMultiColor MakeBodyProps(KeyValuePair<ExtraDecorationDef,ExtraDecorationSettings> decoration, BodyTypeDef pawnBodyType)
    {
        var pawnRenderNodeProperty = MakeBaseProps(decoration);
        pawnRenderNodeProperty.parentTagDef = decoration.Key.drawInHeadSpace ? PawnRenderNodeTagDefOf.Head : PawnRenderNodeTagDefOf.Body;
        pawnRenderNodeProperty.workerClass = typeof(PawnRenderNodeWorker_AttachmentExtraDecorationBody);
        pawnRenderNodeProperty.bodyType = pawnBodyType;
        pawnRenderNodeProperty.useBodyType = decoration.Key.appliesToBodyTypes.Contains(pawnBodyType);

        return pawnRenderNodeProperty;
    }
    
    private PawnRenderNodePropertiesMultiColor MakeHeadProps(KeyValuePair<ExtraDecorationDef,ExtraDecorationSettings> decoration)
    {
        var pawnRenderNodeProperty = MakeBaseProps(decoration);
        pawnRenderNodeProperty.parentTagDef = PawnRenderNodeTagDefOf.Head;
        pawnRenderNodeProperty.workerClass = typeof(PawnRenderNodeWorker_AttachmentExtraDecorationHead);

        return pawnRenderNodeProperty;
    }
    
    private PawnRenderNodePropertiesMultiColor MakeBaseProps(KeyValuePair<ExtraDecorationDef,ExtraDecorationSettings> decoration)
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
            maskDef = decoration.Value.maskDef,
            useMask = decoration.Key.useMask,
            decorationFlags = decoration.Key.decorationFlags,
        };

        return pawnRenderNodeProperty;
    }
}