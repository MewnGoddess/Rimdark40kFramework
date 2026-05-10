using System.Collections.Generic;
using Verse;

namespace Core40k;

public class CompProperties_Decorative : CompProperties
{
    public List<ExtraDecorationDef> extraDecorations = [];
    
    public DecorativeType decorativeType = DecorativeType.Body;
    
    public Dictionary<PawnKindDef, List<ExtraDecorationDef>> extraDecorationsByPawnKindDef = new();
    
    public CompProperties_Decorative()
    {
        compClass = typeof(CompDecorative);
    }
}

public enum DecorativeType
{
    Body = 0,
    Head = 1,
}