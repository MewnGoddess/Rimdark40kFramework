using UnityEngine;
using Verse;

namespace Core40k;

public class PawnRenderNode_AttachmentExtraDecoration : PawnRenderNode
{
    public ExtraDecorationDef ExtraDecorationDef;
    public PawnRenderNode_AttachmentExtraDecoration(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
    {
    }

    public override Graphic GraphicFor(Pawn pawn)
    {
        var propsMulti = (PawnRenderNodePropertiesMultiColor)Props;

        if (!ExtraDecorationDef.useMask)
        {
            return GraphicDatabase.Get<Graphic_Multi>(Props.texPath, ShaderFor(pawn), Props.drawSize, Props.color ?? Color.white);
        }
            
        var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
        return MultiColorUtils.GetGraphic<Graphic_Multi>(propsMulti.texPath, shader, propsMulti.drawSize, propsMulti.color ?? Color.white, propsMulti.colorTwo ?? Color.white, propsMulti.colorThree ?? Color.white, null, Props.texPath + "_mask");

    }
}