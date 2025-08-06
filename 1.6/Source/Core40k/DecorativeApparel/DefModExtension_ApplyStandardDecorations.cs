using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Core40k;

public class DefModExtension_StandardDecorations : DefModExtension
{
    public List<ExtraDecorationDef> extraDecorations = new();
    public Color? defaultPrimaryColor = Color.white;
    public Color? defaultSecondaryColor = Color.white;
    public Color? defaultTertiaryColor = Color.white;
}