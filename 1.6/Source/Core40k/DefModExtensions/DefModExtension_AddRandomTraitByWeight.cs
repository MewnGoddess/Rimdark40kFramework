using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Core40k;

public class DefModExtension_AddRandomTraitByWeight : DefModExtension
{
    public List<WeightedTraitData> possibleTraitsToGive = new();

    public int chanceToGrantTrait = 100;
}