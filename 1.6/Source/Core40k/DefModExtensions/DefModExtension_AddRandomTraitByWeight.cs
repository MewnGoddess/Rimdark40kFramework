using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Core40k;

public class DefModExtension_AddRandomTraitByWeight : DefModExtension
{
    public List<TraitData> possibleTraitsToGive = new List<TraitData>();

    public int chanceToGrantTrait = 100;
}