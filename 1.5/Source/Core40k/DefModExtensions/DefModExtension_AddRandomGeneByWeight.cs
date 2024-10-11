using System.Collections.Generic;
using Verse;

namespace Core40k
{
    public class DefModExtension_AddRandomGeneByWeight : DefModExtension
    {
        public Dictionary<GeneDef, float> possibleGenesToGive;

        public int chanceToGrantGene = 100;
    }
}   