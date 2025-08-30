using System.Collections.Generic;
using Verse;

namespace Core40k;

public class PawnRenderNode_AttachmentApparelMultiColor : PawnRenderNode_Apparel
{
    public PawnRenderNode_AttachmentApparelMultiColor(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
    {
    }

    public override Graphic GraphicFor(Pawn pawn)
    {
        var multiColor = apparel.GetComp<CompMultiColor>();

        string maskPath = null; 
        if (multiColor.MaskDef != null && multiColor.MaskDef.maskExtraFlags.Contains("HasShoulder"))
        {
            maskPath = multiColor.MaskDef?.maskPath;

            if (maskPath != null)
            {
                maskPath += "_Shoulder";
            }
        }

        var texPath = Props.texPath;
        
        if (Props.bodyTypeGraphicPaths != null)
        {
            foreach (var bodyTypeGraphicPath in Props.bodyTypeGraphicPaths)
            {
                if (pawn.story.bodyType != bodyTypeGraphicPath.bodyType)
                {
                    continue;
                }
                texPath = bodyTypeGraphicPath.texturePath;
                break;
            }
        }
        
        var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
        
        return MultiColorUtils.GetGraphic<Graphic_Multi>(texPath, shader, Props.drawSize, multiColor.DrawColor, multiColor.DrawColorTwo, multiColor.DrawColorThree, apparel.Graphic.data, maskPath);
    }
    
    protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
    {
        yield return GraphicFor(pawn);
    }
}