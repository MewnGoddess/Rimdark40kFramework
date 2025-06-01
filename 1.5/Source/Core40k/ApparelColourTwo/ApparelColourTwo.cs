using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class ApparelColourTwo : Apparel
{
    private bool initialColourSet;
    public bool InitialColourSet => initialColourSet;
        
    private Color drawColorOne = Color.white;
        
    private Color originalColorOne = Color.white;
        
    private Color drawColorTwo = Color.red;

    private Color originalColorTwo = Color.red;

    public override Color DrawColor
    {
        get => drawColorOne;
        set
        {
            drawColorOne = value;
            Notify_ColorChanged();
        }
    }

    public override Color DrawColorTwo => drawColorTwo;
    
    public void SetInitialColours(Color colorOne, Color colorTwo)
    {
        drawColorOne = colorOne;
        drawColorTwo = colorTwo;
        SetOriginals();
        initialColourSet = true;
    }
        
    public virtual void SetOriginals()
    {
        originalColorOne = drawColorOne;
        originalColorTwo = drawColorTwo;
    }

    public virtual void SetSecondaryColor(Color color)
    {
        drawColorTwo = color;
        Notify_ColorChanged();
    }

    public virtual void Reset()
    {
        drawColorOne = originalColorOne;
        drawColorTwo = originalColorTwo;
        Notify_ColorChanged();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref initialColourSet, "initialColourSet");
        Scribe_Values.Look(ref originalColorOne, "originalColorOne", Color.black);
        Scribe_Values.Look(ref originalColorTwo, "originalColorTwo", Color.red);
        Scribe_Values.Look(ref drawColorTwo, "drawColorTwo", Color.red);
        Scribe_Values.Look(ref drawColorOne, "drawColorOne", Color.white);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Notify_ColorChanged();
        }
    }
}