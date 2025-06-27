using UnityEngine;
using Verse;

namespace Core40k;

public class ColourPreset : IExposable
{
    public string name = "colour preset";
    public Color primaryColour = Color.white;
    public Color secondaryColour = Color.white;

    public ColourPreset()
    {
            
    }
        
    public ColourPreset(string name, Color primaryColour, Color secondaryColour)
    {
        this.name = name;
        this.primaryColour = primaryColour;
        this.secondaryColour = secondaryColour;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref name, "name");
        Scribe_Values.Look(ref primaryColour, "primaryColour");
        Scribe_Values.Look(ref secondaryColour, "secondaryColour");
    }
}