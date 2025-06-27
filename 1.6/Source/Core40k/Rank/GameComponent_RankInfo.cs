using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace Core40k;

public class GameComponent_RankInfo : GameComponent
{
    //Counts rankDef limits, meaning that each time a rank is unlocked that is limited, it is counted here.
    public Dictionary<RankDef, int> rankLimits = new Dictionary<RankDef, int>();

    public GameComponent_RankInfo(Game game)
    {
    }

    public void PawnResetRanks(List<RankDef> unlockedRanks)
    {
        foreach (var rankDef in unlockedRanks.Where(rankDef => rankDef.colonyLimitOfRank.x > 0 || (rankDef.colonyLimitOfRank.x == 0 && rankDef.colonyLimitOfRank.y > 0)))
        {
            if (!rankLimits.ContainsKey(rankDef))
            {
                continue;
            }
            if (rankLimits[rankDef] == 1)
            {
                rankLimits.Remove(rankDef);
            }
            else
            {
                rankLimits[rankDef] -= 1;
            }
        }
    }

    public void PawnLostRank(RankDef rankDef)
    {
        if (!rankLimits.ContainsKey(rankDef))
        {
            return;
        }
        if (rankLimits[rankDef] == 1)
        {
            rankLimits.Remove(rankDef);
        }
        else
        {
            rankLimits[rankDef] -= 1;
        }
    }

    public void PawnGainedRank(RankDef rankDef)
    {
        if (rankLimits.ContainsKey(rankDef))
        {
            rankLimits[rankDef] += 1;
        }
        else
        {
            rankLimits.Add(rankDef, 1);
        }
    }
        
    public bool CanHaveMoreOfRank(RankDef rankDef)
    {
        var playerPawnAmount = GetColonistForCounting();
                
        var allowedAmount = rankDef.colonyLimitOfRank.y > 0 ? rankDef.colonyLimitOfRank.x + Math.Floor(playerPawnAmount/rankDef.colonyLimitOfRank.y) : rankDef.colonyLimitOfRank.x;
                
        var currentAmount = 0;

        if (rankLimits.ContainsKey(rankDef))
        {
            currentAmount = rankLimits.TryGetValue(rankDef);
        }

        return allowedAmount > currentAmount;
    }
        
    public (bool allowed, int allowedAmount, int currentAmount) CanHaveMoreOfRankWithInfo(RankDef rankDef)
    {
        var playerPawnAmount = GetColonistForCounting();
                
        var allowedAmount = (int)(rankDef.colonyLimitOfRank.y > 0 ? rankDef.colonyLimitOfRank.x + Math.Floor(playerPawnAmount/rankDef.colonyLimitOfRank.y) : rankDef.colonyLimitOfRank.x);
                
        var currentAmount = 0;

        if (rankLimits.ContainsKey(rankDef))
        {
            currentAmount = rankLimits.TryGetValue(rankDef);
        }

        return (allowedAmount > currentAmount, allowedAmount, currentAmount);
    }
        
    public int AllowedAmountOfRank(RankDef rankDef)
    {
        var playerPawnAmount = GetColonistForCounting();
                
        var allowedAmount = rankDef.colonyLimitOfRank.y > 0 ? rankDef.colonyLimitOfRank.x + Math.Floor(playerPawnAmount/rankDef.colonyLimitOfRank.y) : rankDef.colonyLimitOfRank.x;

        return (int)allowedAmount;
    }

    public int CurrentAmountOfRank(RankDef rankDef)
    {
        var currentAmount = 0;

        if (rankLimits.ContainsKey(rankDef))
        {
            currentAmount = rankLimits.TryGetValue(rankDef);
        }

        return currentAmount;
    }
        
    private static int GetColonistForCounting()
    {
        var playerPawnAmount = Find.Maps.Sum(map => map.mapPawns.ColonistCount);
        var caravans = Find.WorldObjects.Caravans.Where(c => c.IsPlayerControlled);
        playerPawnAmount += caravans.SelectMany<Caravan, Pawn>(caravan => caravan.pawns).Count(p => p.Faction != null && p.Faction.IsPlayer);

        return playerPawnAmount;
    }
        
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref rankLimits, "rankLimits");
    }
}