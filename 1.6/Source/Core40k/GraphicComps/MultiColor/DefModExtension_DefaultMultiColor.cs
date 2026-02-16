using System.Collections.Generic;
using Verse;

namespace Core40k;

public class DefModExtension_DefaultMultiColor : DefModExtension
{
    public Dictionary<ColourPresetDef, ColorSelectionType> defaultColorSelection = [];
}

public enum ColorSelectionType
{
    Fail = 0,
    Default = 1,
    TryMatch = 2,
}