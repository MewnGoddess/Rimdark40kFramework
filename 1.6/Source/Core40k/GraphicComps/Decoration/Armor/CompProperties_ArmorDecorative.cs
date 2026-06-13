namespace Core40k;

public class CompProperties_Decorative : CompProperties_DecorationBase
{
    public DecorativeType decorativeType = DecorativeType.Body;
    
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

