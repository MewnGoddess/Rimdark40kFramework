using RimWorld;
using Verse;

namespace Core40k
{
    [DefOf]
    public static class Core40kDefOf
    {
        public static DamageDef BEWH_WarpFlame;

        static Core40kDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Core40kDefOf));
        }
    }
}