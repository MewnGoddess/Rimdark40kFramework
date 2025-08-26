using System.Collections.Generic;
using Verse;

namespace Core40k;

public class DefModExtension_AddRandomGeneByWeight : DefModExtension
{
    public Dictionary<GeneDef, float> possibleGenesToGive = new();

    public int amountToGive = 1;
    
    public int chanceToGrantGene = 100;
}