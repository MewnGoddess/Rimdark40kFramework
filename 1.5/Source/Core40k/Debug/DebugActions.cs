using System.Xml;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;


namespace Core40k
{
    public static class DebugActions
    {
        [DebugAction("RimDark", null, false, false, true, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
        private static void IncrementRankDays(Pawn p)
        {
            if (!p.HasComp<CompRankInfo>())
            {
                return;
            }

            p.GetComp<CompRankInfo>().IncreaseDaysForAllRank();
        }
        
        [DebugAction("RimDark", null, false, false, true, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
        private static void ResetRank(Pawn p)
        {
            if (!p.HasComp<CompRankInfo>())
            {
                return;
            }

            p.GetComp<CompRankInfo>().ResetRanks(null);
        }
    }
}