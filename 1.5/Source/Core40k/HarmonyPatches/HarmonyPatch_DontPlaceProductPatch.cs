using HarmonyLib;
using System;
using System.Collections.Generic;
using VanillaGenesExpanded;
using Verse;

namespace Core40k
{
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public class DontPlaceProductPatch
    {
        public static bool Prefix(ref IEnumerable<Thing> __result, RecipeDef recipeDef)
        {
            if (recipeDef.HasModExtension<DefModExtension_DontPlaceProduct>())
            {
                __result = new List<Thing>();
                return false;
            }
            return true;
        }
    }
}   