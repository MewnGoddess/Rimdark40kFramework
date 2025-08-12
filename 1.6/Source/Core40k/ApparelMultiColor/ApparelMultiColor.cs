using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class ApparelMultiColor : Apparel
{
    private BodyTypeDef originalBodyType = null;

    private bool initialColourSet;
    public bool InitialColourSet => initialColourSet;
        
    private Color drawColorOne = Color.white;
    private Color originalColorOne = Color.white;
    public override Color DrawColor
    {
        get => drawColorOne;
        set
        {
            drawColorOne = value;
            Notify_ColorChanged();
        }
    }
        
    private Color drawColorTwo = Color.white;
    private Color originalColorTwo = Color.white;
    public override Color DrawColorTwo => drawColorTwo;
    
    
    private Color drawColorThree = Color.white;

    private Color originalColorThree = Color.white;
    public Color DrawColorThree => drawColorThree;

    private MaskDef originalMaskDef;
    private MaskDef maskDef;

    public MaskDef MaskDef
    {
        get => maskDef;
        set
        {
            maskDef = value;
            Notify_ColorChanged();
        } 
    }
    
    public override Graphic Graphic
    {
        get
        {
            var path = def.graphicData.texPath;
            var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
            var graphic = MultiColorUtils.GetGraphic<Graphic_Single>(path, shader, def.graphicData.drawSize*0.8f, DrawColor, DrawColorTwo, DrawColorThree, def.graphicData, maskDef?.maskPath);
            return graphic;
        }
    }
    
    public void SetInitialColours(Color colorOne, Color colorTwo, Color? colorThree)
    {
        drawColorOne = colorOne;
        drawColorTwo = colorTwo;
        drawColorThree = colorThree ?? Color.white;
        SetOriginals();
        initialColourSet = true;
    }
        
    public virtual void SetOriginals()
    {
        originalColorOne = drawColorOne;
        originalColorTwo = drawColorTwo;
        originalColorThree = drawColorThree;
        originalMaskDef = maskDef;
    }

    public virtual void SetSecondaryColor(Color color)
    {
        drawColorTwo = color.a == 0 ? drawColorOne : color;
        Notify_ColorChanged();
    }
    
    public virtual void SetTertiaryColor(Color color)
    {
        drawColorThree = color.a == 0 ? drawColorTwo : color;
        Notify_ColorChanged();
    }

    public virtual void Reset()
    {
        drawColorOne = originalColorOne;
        drawColorTwo = originalColorTwo;
        drawColorThree = originalColorThree;
        maskDef = originalMaskDef;
        Notify_ColorChanged();
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        if (!def.HasModExtension<DefModExtension_ForcesBodyType>())
        {
            return;
        }

        var defMod = def.GetModExtension<DefModExtension_ForcesBodyType>();
        
        if (pawn.story.bodyType != defMod.forcedBodyType)
        {
            originalBodyType = pawn.story.bodyType;
            pawn.story.bodyType = defMod.forcedBodyType;
        }
        base.Notify_Equipped(pawn);
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        if (!def.HasModExtension<DefModExtension_ForcesBodyType>())
        {
            return;
        }
        
        if (originalBodyType != null)
        {
            pawn.story.bodyType = originalBodyType;
        }
        base.Notify_Unequipped(pawn);
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref initialColourSet, "initialColourSet");
        Scribe_Values.Look(ref originalColorOne, "originalColorOne", Color.white);
        Scribe_Values.Look(ref originalColorTwo, "originalColorTwo", Color.white);
        Scribe_Values.Look(ref originalColorThree, "originalColorThree", Color.white);
        Scribe_Values.Look(ref drawColorOne, "drawColorOne", Color.white);
        Scribe_Values.Look(ref drawColorTwo, "drawColorTwo", Color.white);
        Scribe_Values.Look(ref drawColorThree, "drawColorThree", Color.white);
        Scribe_Defs.Look(ref originalMaskDef, "originalMaskDef");
        Scribe_Defs.Look(ref maskDef, "maskDef");
        Scribe_Defs.Look(ref originalBodyType, "originalBodyType");
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Notify_ColorChanged();
        }
    }
}