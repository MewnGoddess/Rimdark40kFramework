using System.Collections.Generic;
using Verse;

namespace Core40k;

public class ExtraDecorationPresetDef : Def
{
    public List<ThingDef> appliesTo = new List<ThingDef>();
    public List<PresetData> presetData = new();
}