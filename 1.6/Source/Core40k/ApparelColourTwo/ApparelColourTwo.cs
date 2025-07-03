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
        
    private Color drawColorTwo = Color.white;

    private Color originalColorTwo = Color.white;

    private BodyTypeDef originalBodyType = null;
    
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
        Scribe_Values.Look(ref drawColorTwo, "drawColorTwo", Color.white);
        Scribe_Values.Look(ref drawColorOne, "drawColorOne", Color.white);
        Scribe_Defs.Look(ref originalBodyType, "originalBodyType");
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Notify_ColorChanged();
        }
    }
}