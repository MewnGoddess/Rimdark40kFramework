using System.Collections.Generic;
using Verse;

namespace Core40k;

public class ExtraDecorationPresetDef : Def
{
    public ThingDef appliesTo = null;
    public List<PresetData> presetData = new();
}