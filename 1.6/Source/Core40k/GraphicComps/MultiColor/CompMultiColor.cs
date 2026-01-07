using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class CompMultiColor : CompGraphicParent
{
    public void TempSetInitialValues(ApparelMultiColor multiColor)
    {
        drawColorOne = multiColor.DrawColor;
        originalColorOne = multiColor.DrawColor;
        
        drawColorTwo = multiColor.DrawColorTwo;
        originalColorTwo = multiColor.DrawColorTwo;
        
        drawColorThree = multiColor.DrawColorThree;
        originalColorThree = multiColor.DrawColorThree;
        
        maskDef = multiColor.MaskDef;
        originalMaskDef = multiColor.MaskDef;
    }
    
    public void TempSetInitialValues(WeaponMultiColor multiColor)
    {
        drawColorOne = multiColor.DrawColor;
        originalColorOne = multiColor.DrawColor;
        
        drawColorTwo = multiColor.DrawColorTwo;
        originalColorTwo = multiColor.DrawColorTwo;
        
        drawColorThree = multiColor.DrawColorThree;
        originalColorThree = multiColor.DrawColorThree;
    }
    
    public CompProperties_MultiColor Props => (CompProperties_MultiColor)props; 

    private ThingDef thingDef => parent.def;
    
    public Pawn Wearer
    {
        get
        {
            if (ParentHolder is not Pawn_ApparelTracker pawn_ApparelTracker)
            {
                return null;
            }
            return pawn_ApparelTracker.pawn;
        }
    }
    
    private bool isApparel => parent is Apparel;
    
    private BodyTypeDef originalBodyType = null;
        
    private Color drawColorOne = Color.white;
    private Color originalColorOne = Color.white;

    public Color DrawColor
    {
        get => drawColorOne;
        set
        {
            drawColorOne = value.a == 0 ? Color.white : value;
            Notify_GraphicChanged();
        }
    }

    private Color drawColorTwo = Color.white;
    private Color originalColorTwo = Color.white;
    public Color DrawColorTwo
    {
        get => drawColorTwo;
        set
        {
            drawColorTwo = value.a == 0 ? drawColorOne : value;
            Notify_GraphicChanged();
        }
    }
    
    
    private Color drawColorThree = Color.white;
    private Color originalColorThree = Color.white;
    public Color DrawColorThree
    {
        get => drawColorThree;
        set
        {
            drawColorThree = value.a == 0 ? drawColorTwo : value;
            Notify_GraphicChanged();
        }
    }

    private MaskDef originalMaskDef;
    private MaskDef maskDef;
    public MaskDef MaskDef
    {
        get => maskDef;
        set
        {
            maskDef = value;
            Notify_GraphicChanged();
        } 
    }

    private bool recacheGraphics = true;
    public bool RecacheGraphics => recacheGraphics;
    
    private Graphic_Multi cachedGraphicMulti;
    public Graphic_Multi CachedGraphicMulti
    {
        get => cachedGraphicMulti;
        set
        {
            cachedGraphicMulti = value;
            recacheGraphics = false;
            if (isApparel)
            {
                apparelGraphicRecord = new ApparelGraphicRecord(cachedGraphicMulti, parent as Apparel);
            }
        }
    }

    private ApparelGraphicRecord? apparelGraphicRecord;
    public ApparelGraphicRecord ApparelGraphicRecord
    {
        get
        {
            if (!isApparel)
            {
                return new ApparelGraphicRecord(null, null);
            }
            apparelGraphicRecord ??= new ApparelGraphicRecord(CachedGraphicMulti, parent as Apparel);
            return apparelGraphicRecord.Value;
        }
    }
    
    
    private bool recacheSingleGraphics = true;
    public bool RecacheSingleGraphics => recacheSingleGraphics;
    
    private Graphic cachedGraphic;
    public Graphic Graphic => cachedGraphic;

    public void SetSingleGraphic()
    {
        recacheSingleGraphics = false;
        var path = thingDef.graphicData.texPath;
        var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
        var drawMult = isApparel ? 0.9f : 1f;
        var graphic = MultiColorUtils.GetGraphic<Graphic_Single>(path, shader, thingDef.graphicData.drawSize*drawMult, DrawColor, DrawColorTwo, DrawColorThree, null, maskDef?.maskPath);
        cachedGraphic = new Graphic_RandomRotated(graphic, 35f);
    }

    public List<ApparelMultiColorTabDef> ApparelMultiColorTabDefs => Props.tabDefs;
    
    public override void InitialSetup()
    {
        InitialColors();
        base.InitialSetup();
        recacheGraphics = true;
    }

    public virtual void InitialColors()
    {
        drawColorOne = Props.defaultPrimaryColor ?? (thingDef.MadeFromStuff ? thingDef.GetColorForStuff(parent.Stuff) : Color.white);
        drawColorTwo = Props.defaultSecondaryColor ?? Color.white;
        drawColorThree = Props.defaultTertiaryColor ?? Color.white;
    }
    
    public void SetColors(Color colorOne, Color colorTwo, Color? colorThree)
    {
        drawColorOne = colorOne;
        drawColorTwo = colorTwo;
        drawColorThree = colorThree ?? Color.white;
        SetOriginals();
    }
    
    public void SetColors(ColourPresetDef preset)
    {
        drawColorOne = preset.primaryColour;
        drawColorTwo = preset.secondaryColour;
        drawColorThree = preset.tertiaryColour ?? preset.secondaryColour;
        SetOriginals();
    }
    
    public override void SetOriginals()
    {
        originalColorOne = drawColorOne;
        originalColorTwo = drawColorTwo;
        originalColorThree = drawColorThree;
        originalMaskDef = maskDef;
        Notify_GraphicChanged();
    }

    public override void Reset()
    {
        drawColorOne = originalColorOne;
        drawColorTwo = originalColorTwo;
        drawColorThree = originalColorThree;
        maskDef = originalMaskDef;
        Notify_GraphicChanged();
    }
    
    public override void Notify_GraphicChanged()
    {
        recacheGraphics = true;
        recacheSingleGraphics = true;
        base.Notify_GraphicChanged();
    }
    
    public override void Notify_Equipped(Pawn pawn)
    {
        Notify_GraphicChanged();
        base.Notify_Equipped(pawn);
    }
    
    public override void PostExposeData()
    {
        Scribe_Values.Look(ref originalColorOne, "originalColorOne", Color.white);
        Scribe_Values.Look(ref originalColorTwo, "originalColorTwo", Color.white);
        Scribe_Values.Look(ref originalColorThree, "originalColorThree", Color.white);
        Scribe_Values.Look(ref drawColorOne, "drawColorOne", Color.white);
        Scribe_Values.Look(ref drawColorTwo, "drawColorTwo", Color.white);
        Scribe_Values.Look(ref drawColorThree, "drawColorThree", Color.white);
        Scribe_Defs.Look(ref originalMaskDef, "originalMaskDef");
        Scribe_Defs.Look(ref maskDef, "maskDef");
        Scribe_Defs.Look(ref originalBodyType, "originalBodyType");
        
        base.PostExposeData();
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Notify_GraphicChanged();
        }
    }
}