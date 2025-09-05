using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace Core40k;

[HarmonyPatch(typeof(Faction), "TryMakeInitialRelationsWith")]
public class InitialGoodwillPatch
{
    public static void Postfix(Faction other, List<FactionRelation> ___relations, Faction __instance)
    {
        if (__instance == null || other == null)
        {
            return;
        }

        DefModExtension_InitialGoodwill defMod;
        
        if (__instance.def.HasModExtension<DefModExtension_InitialGoodwill>())
        {
            defMod = __instance.def.GetModExtension<DefModExtension_InitialGoodwill>();
        }
        else if (other.def.HasModExtension<DefModExtension_InitialGoodwill>())
        {
            defMod = other.def.GetModExtension<DefModExtension_InitialGoodwill>();
        }
        else
        {
            return;
        }

        if ((__instance.IsPlayer || other.IsPlayer) && !defMod.applyToPlayer)
        {
            return;
        }

        if (defMod.onlyApplyToPlayer && !(__instance.IsPlayer || other.IsPlayer))
        {
            return;
        }

        __instance.RelationWith(other).baseGoodwill = defMod.initialGoodwill;
        other.RelationWith(__instance).baseGoodwill = defMod.initialGoodwill;
    }
}