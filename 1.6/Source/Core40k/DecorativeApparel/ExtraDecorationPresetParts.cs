using UnityEngine;
using Verse;

namespace Core40k;

public class ExtraDecorationPresetParts : IExposable
{
    public string extraDecorationDefs = null;
    public bool flipped = false;
    public Color colour = Color.white;
    
    public ExtraDecorationPresetParts()
    {
        
    }
    
    public ExtraDecorationPresetParts(ExtraDecorationDef extraDecorationDefs, bool flipped, Color colour)
    {
        this.extraDecorationDefs = extraDecorationDefs.defName;
        this.flipped = flipped;
        this.colour = colour;
    }
    
    public void ExposeData()
    {
        Scribe_Values.Look(ref extraDecorationDefs, "extraDecorationDefs");
        Scribe_Values.Look(ref flipped, "isFlipped");
        Scribe_Values.Look(ref colour, "colour");
    }
}