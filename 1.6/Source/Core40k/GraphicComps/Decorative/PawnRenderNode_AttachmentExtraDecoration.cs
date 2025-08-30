using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class PawnRenderNode_AttachmentExtraDecoration : PawnRenderNode
{
    public PawnRenderNode_AttachmentExtraDecoration(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
    {
    }

    public override Graphic GraphicFor(Pawn pawn)
    {
        var propsMulti = (PawnRenderNodePropertiesMultiColor)Props;

        if (!propsMulti.useMask)
        {
            return GraphicDatabase.Get<Graphic_Multi>(Props.texPath, ShaderFor(pawn), Props.drawSize, Props.color ?? Color.white);
        }

        var shader = propsMulti.shaderTypeDef.Shader;

        string maskPath;

        var texPath = propsMulti.texPath;

        var pawnFlags = GetPawnFlags(pawn);

        var additionalMaskPath = "";
        var decorationFlag = propsMulti.decorationFlags.Where(flag => pawnFlags.Contains(flag.flag)).OrderBy(d => d.priority).FirstOrDefault();
        if (decorationFlag != null)
        {
            texPath = decorationFlag.newTexPath;
            shader = decorationFlag.shaderType?.Shader ?? shader;
            additionalMaskPath += decorationFlag.maskPathAddition;
        }
        
        if (propsMulti.maskDef != null && propsMulti.maskDef.maskPath != null)
        {
            maskPath = propsMulti.maskDef.maskPath + additionalMaskPath;
            if (propsMulti.useBodyType)
            {
                maskPath += "_" + propsMulti.bodyType.defName;
            }
        }
        else
        {
            maskPath = propsMulti.texPath + additionalMaskPath;;
            if (propsMulti.useBodyType)
            {
                maskPath += "_" + propsMulti.bodyType.defName;
            }
            maskPath += "_mask";
        }
        
        return MultiColorUtils.GetGraphic<Graphic_Multi>(texPath, shader, propsMulti.drawSize, propsMulti.color ?? Color.white, propsMulti.colorTwo ?? Color.white, propsMulti.colorThree ?? Color.white, null, maskPath);

    }

    private static List<string> GetPawnFlags(Pawn pawn)
    {
        var flags = new List<string>();

        foreach (var wornApparel in pawn.apparel.WornApparel.Where(wornApparel => wornApparel.def.HasModExtension<DefModExtension_DecorationFlags>()))
        {
            flags.AddRange(wornApparel.def.GetModExtension<DefModExtension_DecorationFlags>().flags);
        }
        
        foreach (var equipment in pawn.equipment.AllEquipmentListForReading.Where(equipment => equipment.def.HasModExtension<DefModExtension_DecorationFlags>()))
        {
            flags.AddRange(equipment.def.GetModExtension<DefModExtension_DecorationFlags>().flags);
        }

        foreach (var pawnGene in pawn.genes.GenesListForReading.Where(pawnGene => pawnGene.def.HasModExtension<DefModExtension_DecorationFlags>()))
        {
            flags.AddRange(pawnGene.def.GetModExtension<DefModExtension_DecorationFlags>().flags);
        }
        
        foreach (var pawnTraits in pawn.story.traits.allTraits.Where(pawnTraits => pawnTraits.def.HasModExtension<DefModExtension_DecorationFlags>()))
        {
            flags.AddRange(pawnTraits.def.GetModExtension<DefModExtension_DecorationFlags>().flags);
        }
        
        foreach (var pawnHediff in pawn.health.hediffSet.hediffs.Where(pawnHediff => pawnHediff.def.HasModExtension<DefModExtension_DecorationFlags>()))
        {
            flags.AddRange(pawnHediff.def.GetModExtension<DefModExtension_DecorationFlags>().flags);
        }
        
        return flags;
    }
}