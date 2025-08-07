using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Core40k;

public class DefModExtension_AddRandomTraitByWeight : DefModExtension
{
    public Dictionary<TraitData, float> possibleTraitsToGive = new Dictionary<TraitData, float>();

    public int chanceToGrantTrait = 100;
}