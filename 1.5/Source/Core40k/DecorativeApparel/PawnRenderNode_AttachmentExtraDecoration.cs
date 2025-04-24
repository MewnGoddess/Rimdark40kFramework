using UnityEngine;
using Verse;

namespace Core40k
{
    public class PawnRenderNode_AttachmentExtraDecoration : PawnRenderNode
    {
        public ExtraDecorationDef ExtraDecorationDef;
        public PawnRenderNode_AttachmentExtraDecoration(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            Props.color ??= Color.white;

            if (ExtraDecorationDef.useMask)
            {
                return GraphicDatabase.Get<Graphic_Multi>(Props.texPath, ShaderFor(pawn), Props.drawSize, Props.color.Value, Props.color.Value, null, Props.texPath + "_m");
            }
            
            return GraphicDatabase.Get<Graphic_Multi>(Props.texPath, ShaderFor(pawn), Props.drawSize, Props.color.Value);
        }
    }
}