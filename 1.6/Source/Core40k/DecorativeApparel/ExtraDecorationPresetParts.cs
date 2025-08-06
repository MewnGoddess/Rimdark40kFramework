using UnityEngine;
using Verse;

namespace Core40k;

public class ExtraDecorationPresetParts : IExposable
{
    public string extraDecorationDefs = null;
    public bool flipped = false;
    public Color colour = Color.white;
    public Color colourTwo = Color.white;
    public Color colourThree = Color.white;
    
    public ExtraDecorationPresetParts()
    {
        
    }
    
    public ExtraDecorationPresetParts(ExtraDecorationDef extraDecorationDefs, bool flipped, Color colour, Color colourTwo, Color colourThree)
    {
        this.extraDecorationDefs = extraDecorationDefs.defName;
        this.flipped = flipped;
        this.colour = colour;
        this.colourTwo = colourTwo;
        this.colourThree = colourThree;
    }
    
    public void ExposeData()
    {
        Scribe_Values.Look(ref extraDecorationDefs, "extraDecorationDefs");
        Scribe_Values.Look(ref flipped, "isFlipped");
        Scribe_Values.Look(ref colour, "colour");
        Scribe_Values.Look(ref colourTwo, "colourTwo");
        Scribe_Values.Look(ref colourThree, "colourThree");
    }
}