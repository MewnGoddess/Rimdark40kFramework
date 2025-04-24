using UnityEngine;
using Verse;

namespace Core40k;

public class ExtraDecorationPresetParts : IExposable
{
    public string extraDecorationDefs = null;
    public bool isFlipped = false;
    public Color colour = Color.white;
    
    public ExtraDecorationPresetParts()
    {
        
    }
    
    public ExtraDecorationPresetParts(ExtraDecorationDef extraDecorationDefs, bool isFlipped, Color colour)
    {
        this.extraDecorationDefs = extraDecorationDefs.defName;
        this.isFlipped = isFlipped;
        this.colour = colour;
    }
    
    public void ExposeData()
    {
        Scribe_Values.Look(ref extraDecorationDefs, "extraDecorationDefs");
        Scribe_Values.Look(ref isFlipped, "isFlipped");
        Scribe_Values.Look(ref colour, "colour");
    }
}