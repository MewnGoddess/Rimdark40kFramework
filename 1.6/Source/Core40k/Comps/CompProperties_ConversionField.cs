using RimWorld;
using Verse;

namespace Core40k;

public class CompProperties_ConversionField : CompProperties_Shield
{
    public CompProperties_ConversionField()
    {
        compClass = typeof(Comp_ConversionField);
        blocksRangedWeapons = false;
    }
}
