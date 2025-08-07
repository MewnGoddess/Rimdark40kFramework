using Verse;

namespace Core40k;

public class DefModExtension_AmmoChanger : DefModExtension
{
    public ResearchProjectDef unlockedBy = null;
    public float? effectiveRange;
    public float? warmupTime;
    public int? shotsPerBurst;
}