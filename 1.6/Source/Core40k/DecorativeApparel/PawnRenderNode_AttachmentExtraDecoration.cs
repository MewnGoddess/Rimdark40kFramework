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

        return MultiColorUtils.GetGraphic<Graphic_Multi>(propsMulti.texPath, Core40kDefOf.BEWH_CutoutThreeColor.Shader, propsMulti.drawSize, propsMulti.color ?? Color.white, propsMulti.colorTwo ?? Color.white, propsMulti.colorThree ?? Color.white, null, propsMulti.maskDef?.maskPath ?? propsMulti.texPath + "_mask");

    }
}