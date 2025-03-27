using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Genes40k
{
    [HarmonyPatch(typeof(PawnRenderTree), "ProcessApparel")]
    public class PowerArmorBodyDecorationPatch
    {
        public static void Postfix(Apparel ap, Dictionary<PawnRenderNodeTagDef, PawnRenderNode> ___nodesByTag, Dictionary<PawnRenderNodeTagDef, List<PawnRenderNode>> ___tmpChildTagNodes, PawnRenderTree __instance, Pawn ___pawn)
        {
            if (ap is not BodyDecorativeApparelColourTwo decorativeApparel)
            {
                return;
            }
            
            var defaultLayerValue = 76f;
            
            foreach (var decoration in decorativeApparel.ExtraDecorationDefs)
            {
                var defaultLayer = new DrawData.RotationalData
                {
                    layer = defaultLayerValue,
                };
                
                var eastLayer = new DrawData.RotationalData
                {
                    layer = defaultLayerValue + decoration.Key.layerOffsets.TryGetValue(Rot4.East),
                    rotation = Rot4.East,
                    flip = decoration.Value,
                };
                
                var westLayer = new DrawData.RotationalData
                {
                    layer = defaultLayerValue + decoration.Key.layerOffsets.TryGetValue(Rot4.West),
                    rotation = Rot4.West,
                    flip = decoration.Value,
                };

                var southLayer = new DrawData.RotationalData
                {
                    layer = defaultLayerValue + decoration.Key.layerOffsets.TryGetValue(Rot4.South),
                    rotation = Rot4.South,
                };
                
                var northLayer = new DrawData.RotationalData
                {
                    layer = defaultLayerValue + decoration.Key.layerOffsets.TryGetValue(Rot4.North),
                    rotation = Rot4.North,
                };
                
                var rotationalData = new DrawData.RotationalData[5];
                rotationalData[0] = defaultLayer;
                rotationalData[1] = eastLayer;
                rotationalData[2] = westLayer;
                rotationalData[3] = southLayer;
                rotationalData[4] = northLayer;
                
                var pawnRenderNodeProperty = new PawnRenderNodeProperties
                {
                    nodeClass = typeof(PawnRenderNode_AttachmentExtraDecoration),
                    workerClass = typeof(PawnRenderNodeWorker_AttachmentExtraDecorationBody),
                    texPath = decoration.Key.drawnTextureIconPath,
                    shaderTypeDef = decoration.Key.shaderType,                                                                      
                    parentTagDef = PawnRenderNodeTagDefOf.Body,
                    drawData = DrawData.NewWithData(rotationalData),
                    flipGraphic = decoration.Value,
                    color = decorativeApparel.ExtraDecorationColours[decoration.Key],
                };
                
                var pawnRenderNode = (PawnRenderNode_AttachmentExtraDecoration)Activator.CreateInstance(typeof(PawnRenderNode_AttachmentExtraDecoration), ___pawn, pawnRenderNodeProperty, __instance);
                pawnRenderNode.Props.parentTagDef = PawnRenderNodeTagDefOf.Body;
                
                AddChild(pawnRenderNode, null, ___nodesByTag, ___tmpChildTagNodes, __instance.rootNode);
            }
        }
        
        private static void AddChild(PawnRenderNode child, PawnRenderNode parent, Dictionary<PawnRenderNodeTagDef, PawnRenderNode> nodesByTag, Dictionary<PawnRenderNodeTagDef, List<PawnRenderNode>> tmpChildTagNodes, PawnRenderNode rootNode)
        {
            if (parent == null)
            {
                parent = child.Props.parentTagDef == null || !nodesByTag.TryGetValue(child.Props.parentTagDef, out var value) ? rootNode : value;
            }
            if (parent.Props.tagDef != null)
            {
                if (tmpChildTagNodes.TryGetValue(parent.Props.tagDef, out var value2))
                {
                    value2.Add(child);
                }
                else
                {
                    tmpChildTagNodes.Add(parent.Props.tagDef, new List<PawnRenderNode> { child });
                }
            }
            child.parent = parent;
        }
    }
}