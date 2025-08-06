﻿using LudeonTK;
using Verse;

namespace Core40k;

public static class DebugActions
{
    [DebugAction("RimDark", "Increment all rank days by 1", false, false, false, false, false,0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
    private static void IncrementRankDays(Pawn p)
    {
        if (!p.HasComp<CompRankInfo>())
        {
            return;
        }

        p.GetComp<CompRankInfo>().IncreaseDaysForAllRank();
    }
        
    [DebugAction("RimDark", "Reset all ranks", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
    private static void ResetRank(Pawn p)
    {
        if (!p.HasComp<CompRankInfo>())
        {
            return;
        }

        p.GetComp<CompRankInfo>().ResetRanks(null);
    }
}