using UnityEngine;
using Verse;

namespace Core40k;

public class ColourPreset : IExposable
{
    public string name = "colour preset";
    public PresetType appliesToKind = PresetType.None;
    public Color primaryColour = Color.white;
    public Color secondaryColour = Color.white;
    public Color tertiaryColour = new Color(0,0,0,0);

    public ColourPreset()
    {
            
    }
        
    public ColourPreset(string name, Color primaryColour, Color secondaryColour, Color tertiaryColour, PresetType appliesToKind)
    {
        this.name = name;
        this.primaryColour = primaryColour;
        this.secondaryColour = secondaryColour;
        this.tertiaryColour = tertiaryColour;
        this.appliesToKind = appliesToKind;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref name, "name");
        Scribe_Values.Look(ref primaryColour, "primaryColour");
        Scribe_Values.Look(ref secondaryColour, "secondaryColour");
        Scribe_Values.Look(ref tertiaryColour, "tertiaryColour");
        Scribe_Values.Look(ref appliesToKind, "appliesToKind");
    }
}