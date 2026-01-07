using System;
using UnityEngine;
using Verse;

namespace Core40k;

[Obsolete]
public class WeaponMultiColor : ThingWithComps
{
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
    
    private CompMultiColor CompMultiColor => GetComp<CompMultiColor>();

    public override Graphic Graphic
    {
        get
        {
            var path = def.graphicData.texPath;
            var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
            return MultiColorUtils.GetGraphic<Graphic_Single>(path, shader, def.graphicData.drawSize, CompMultiColor.DrawColor, CompMultiColor.DrawColorTwo, CompMultiColor.DrawColorThree, def.graphicData, null);
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