using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class ApparelMultiColor : Apparel
{
    private BodyTypeDef originalBodyType = null;

    private bool initialColourSet;
        
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
    
    private Graphic_Multi cachedGraphicMulti;
    public Graphic_Multi CachedGraphicMulti
    {
        get => cachedGraphicMulti;
        set
        {
            cachedGraphicMulti = value;
            apparelGraphicRecord = new ApparelGraphicRecord(cachedGraphicMulti, this);
        }
    }

    private ApparelGraphicRecord? apparelGraphicRecord;
    public ApparelGraphicRecord ApparelGraphicRecord
    {
        get
        {
            apparelGraphicRecord ??= new ApparelGraphicRecord(CachedGraphicMulti, this);
            return apparelGraphicRecord.Value;
        }
    }
    
    private CompMultiColor CompMultiColor => GetComp<CompMultiColor>();

    public override Graphic Graphic
    {
        get
        {
            var path = def.graphicData.texPath;
            var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
            return MultiColorUtils.GetGraphic<Graphic_Single>(path, shader, def.graphicData.drawSize*0.8f, CompMultiColor.DrawColor, CompMultiColor.DrawColorTwo, CompMultiColor.DrawColorThree, def.graphicData, CompMultiColor.MaskDef?.maskPath);
        }
    }

    private bool movedToComp = false;
    
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
        
        Scribe_Values.Look(ref movedToComp, "movedToComp");
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Notify_ColorChanged();
            if (!movedToComp && this.HasComp<CompMultiColor>())
            {
                var multiColor = GetComp<CompMultiColor>();
                multiColor.TempSetInitialValues(this);
                movedToComp = true;
            }
        }
    }
}