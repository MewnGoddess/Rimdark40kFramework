using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Core40k;

public class ColourPresetDef : Def
{
    public Color primaryColour = Color.white;
    public Color secondaryColour = Color.white;
    public Color tertiaryColour = new Color(0,0,0,0);
    
    public List<string> appliesTo = new List<string>();
}