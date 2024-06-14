using RimWorld;
using Verse;

namespace Core40k
{
    [DefOf]
    public static class Core40kDefOf
    {
        static Core40kDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Core40kDefOf));
        }
    }
}