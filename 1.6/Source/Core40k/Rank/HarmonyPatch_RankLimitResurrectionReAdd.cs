using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace Core40k;

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
            if (gameComp.CanHaveMoreOfRank(rank))
            {
                gameComp.PawnGainedRank(rank);
            }
            else
            {
                comp.RemoveRank(rank, false);
            }
        }
    }
}