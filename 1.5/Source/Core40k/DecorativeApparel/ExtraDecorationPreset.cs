using System.Collections.Generic;
using Verse;

namespace Core40k;

public class ExtraDecorationPreset : IExposable
{
    public string name = "extra decoration preset";
    public string appliesTo = null;
    public List<ExtraDecorationPresetParts> extraDecorationPresetParts = new List<ExtraDecorationPresetParts>();
    
    public ExtraDecorationPreset()
    {
            
    }
    
    public ExtraDecorationPreset(string name, List<ExtraDecorationPresetParts> extraDecorationPresetParts, ThingDef appliesTo)
    {
        this.name = name;
        this.appliesTo = appliesTo.defName;
        this.extraDecorationPresetParts = extraDecorationPresetParts;
    }
    
    public void ExposeData()
    {
        Scribe_Collections.Look(ref extraDecorationPresetParts, "extraDecorationPreset");
        Scribe_Values.Look(ref appliesTo, "appliesTo");
        Scribe_Values.Look(ref name, "name");
    }
}