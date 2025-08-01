using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

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

    public override Graphic Graphic
    {
        get
        {
            var path = def.graphicData.texPath;
            var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
            var graphic = MultiColorUtils.GetGraphic<Graphic_Single>(path, shader, def.graphicData.drawSize, DrawColor, DrawColorTwo, DrawColorThree, def.graphicData, null);
            return graphic;
        }
    }
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        SetInitialColours(def.GetColorForStuff(Stuff));
    }

    public void SetInitialColours(Color colorOne, Color? colorTwo = null, Color? colorThree = null)
    {
        drawColorOne = colorOne;
        drawColorTwo = colorTwo ?? Color.white;
        drawColorThree = colorThree ?? Color.white;
        SetOriginals();
        initialColourSet = true;
    }
        
    public virtual void SetOriginals()
    {
        originalColorOne = drawColorOne;
        originalColorTwo = drawColorTwo;
        originalColorThree = drawColorThree;
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
        Notify_ColorChanged();
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
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Notify_ColorChanged();
        }
    }
}