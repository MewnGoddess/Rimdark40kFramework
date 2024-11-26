using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace Core40k
{
    [HarmonyPatch(typeof(ResurrectionUtility), "TryResurrect")]
    public class RankLimitResurrectionReAddPatch
    {
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (!__result || !pawn.HasComp<CompRankInfo>())
            {
                return;
            }

            var comp = pawn.GetComp<CompRankInfo>();
            var gameComp = Current.Game.GetComponent<GameComponent_RankInfo>();

            foreach (var rank in comp.UnlockedRanks.Where(rank => rank.colonyLimitOfRank.x > 0 || (rank.colonyLimitOfRank.x == 0 && rank.colonyLimitOfRank.y > 0)))
            {
                if (gameComp.rankLimits.ContainsKey(rank))
                {
                    gameComp.rankLimits[rank] += 1;
                }
                else
                {
                    gameComp.rankLimits.Add(rank, 1);
                }
            }
        }
    }
}