using System.Collections.Generic;
using Verse;

namespace Core40k;

public class PawnRenderNode_AttachmentBackpack : PawnRenderNode_Apparel
{
    public PawnRenderNode_AttachmentBackpack(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
    {
    }
        
    public override Graphic GraphicFor(Pawn pawn)
    {
        var multiColor = apparel.GetComp<CompMultiColor>();

        var texPath = Props.texPath;
        string maskPath = null;

        var jumpPackVisual = ModsConfig.RoyaltyActive 
                             && pawn.apparel.WornApparel.Any(wornApparel => wornApparel.def == Core40kDefOf.Apparel_PackJump) 
                             && apparel.def.HasModExtension<DefModExtension_HideJumpPack>() 
                             && apparel.def.GetModExtension<DefModExtension_HideJumpPack>().changeBackpackVisual;

        if (jumpPackVisual)
        {
            texPath += "_Jump";
        }

        if (multiColor.MaskDef != null)
        {
            maskPath = multiColor.MaskDef.maskPath;
            
            var backpackMask = multiColor.MaskDef.maskExtraFlags.Contains("HasBackpack");
            var jumppackMask = multiColor.MaskDef.maskExtraFlags.Contains("HasJumppack");
            
            if (jumpPackVisual)
            {
                if (maskPath != null && jumppackMask)
                {
                    maskPath += "_Backpack_Jump";
                }
                else
                {
                    maskPath = null;
                }
            }
            else
            {
                if (maskPath != null && backpackMask)
                {
                    maskPath += "_Backpack";
                }
                else
                {
                    maskPath = null;
                }
            }
        }
        
        var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
        
        return MultiColorUtils.GetGraphic<Graphic_Multi>(texPath, shader, Props.drawSize, multiColor.DrawColor, multiColor.DrawColorTwo, multiColor.DrawColorThree, apparel.def.graphicData, maskPath);
    }
    
    protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
    {
        yield return GraphicFor(pawn);
    }
}