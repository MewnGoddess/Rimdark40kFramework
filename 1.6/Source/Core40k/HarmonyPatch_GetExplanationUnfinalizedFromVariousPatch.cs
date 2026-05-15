using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(StatWorker), "GetExplanationUnfinalized")]
public static class GetExplanationUnfinalizedFromVariousPatch
{
    private static GameComponent_CoreUtils coreUtils;
    private static GameComponent_CoreUtils CoreUtils => coreUtils ??= Current.Game.GetComponent<GameComponent_CoreUtils>();
    
    public static void Postfix(ref string __result, StatWorker __instance, StatRequest req)
    {
        if (req.Thing is not Pawn pawn)
        {
            return;
        }
        
        var stringbuilder = new StringBuilder();
        stringbuilder.Append(__result);
        __result = GetExplanationForX(stringbuilder, req, __instance, pawn).ToString();
    }

    public static StringBuilder GetExplanationForX(StringBuilder stringBuilder, StatRequest req, StatWorker statWorker, Pawn pawn)
    {
        StatDef stat = null;
        
        //Rank
        var appendOverallRankText = true;
        var rankComp = pawn.GetComp<CompRankInfo>();
        if (rankComp != null && !rankComp.UnlockedRanks.NullOrEmpty())
        {
            stat = statWorker.stat;
            var rankListForReading = rankComp.UnlockedRanks;
            foreach (var rank in rankListForReading)
            {
                if (rank == null)
                {
                    continue;
                }
                var statOffsetFromRank = rank.statOffsets.GetStatOffsetFromList(stat);
                if (statOffsetFromRank != 0f)
                {
                    if (appendOverallRankText)
                    {
                        stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                        appendOverallRankText = false;
                    }
                    stringBuilder.AppendLine("    " + rank.LabelCap + ": " + Core40kUtils.ValueToString(stat, statOffsetFromRank, finalized: false, ToStringNumberSense.Offset));
                }
                var statFactorFromRank = rank.statFactors.GetStatFactorFromList(stat);
                if (statFactorFromRank != 1f)
                {
                    if (appendOverallRankText)
                    {
                        stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                        appendOverallRankText = false;
                    }
                    stringBuilder.AppendLine("    " + rank.LabelCap + ": " + Core40kUtils.ValueToString(stat, statFactorFromRank, finalized: false, ToStringNumberSense.Factor));
                }
                if (rank.conditionalStatAffecters.NullOrEmpty())
                {
                    continue;
                }

                foreach (var conditionalStat in rank.conditionalStatAffecters)
                {
                    if (!conditionalStat.Applies(req))
                    {
                        continue;
                    }
                    var statOffsetFromRankConditional = conditionalStat.statOffsets.GetStatOffsetFromList(stat);
                    if (statOffsetFromRankConditional != 0f)
                    {
                        if (appendOverallRankText)
                        {
                            stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                            appendOverallRankText = false;
                        }
                        stringBuilder.AppendLine("    " + rank.LabelCap + " (" + conditionalStat.Label + "): " + Core40kUtils.ValueToString(stat, statOffsetFromRankConditional, finalized: false, ToStringNumberSense.Offset));
                    }
                    var statFactorFromRankConditional = conditionalStat.statFactors.GetStatFactorFromList(stat);
                    if (statFactorFromRankConditional != 1f)
                    {
                        if (appendOverallRankText)
                        {
                            stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                            appendOverallRankText = false;
                        }
                        stringBuilder.AppendLine("    " + rank.LabelCap + " (" + conditionalStat.Label + "): " + Core40kUtils.ValueToString(stat, statFactorFromRankConditional, finalized: false, ToStringNumberSense.Factor));
                    }
                }
            }
        }
        
        if (!CoreUtils.cachedDecoratives.TryGetValue(pawn, out var cachedDecoratives))
        {
            return stringBuilder;
        }

        if (stat == null)
        {
            stat = statWorker.stat;
        }
        
        //Apparel
        appendOverallRankText = true;
        if (!cachedDecoratives.apparels.NullOrEmpty())
        {
            foreach (var apparel in cachedDecoratives.apparels)
            {
                var comp = apparel.TryGetComp<CompDecorative>();
                if (comp == null)
                {
                    continue;
                }
                foreach (var extraDecoration in comp.ExtraDecorations)
                {
                    var statOffsetFromRank = extraDecoration.Key.statOffsets.GetStatOffsetFromList(stat);
                    if (statOffsetFromRank != 0f)
                    {
                        if (appendOverallRankText)
                        {
                            stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_ArmorDeco".Translate());
                            appendOverallRankText = false;
                        }
                        stringBuilder.AppendLine("    " + extraDecoration.Key.LabelCap + ": " + Core40kUtils.ValueToString(stat, statOffsetFromRank, finalized: false, ToStringNumberSense.Offset));
                    }
                    var statFactorFromRank = extraDecoration.Key.statFactors.GetStatFactorFromList(stat);
                    if (statFactorFromRank != 1f)
                    {
                        if (appendOverallRankText)
                        {
                            stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_ArmorDeco".Translate());
                            appendOverallRankText = false;
                        }
                        stringBuilder.AppendLine("    " + extraDecoration.Key.LabelCap + ": " + Core40kUtils.ValueToString(stat, statFactorFromRank, finalized: false, ToStringNumberSense.Factor));
                    }
                    if (extraDecoration.Key.conditionalStatAffecters.NullOrEmpty())
                    {
                        continue;
                    }

                    foreach (var conditionalStat in extraDecoration.Key.conditionalStatAffecters)
                    {
                        if (!conditionalStat.Applies(req))
                        {
                            continue;
                        }
                        var statOffsetFromRankConditional = conditionalStat.statOffsets.GetStatOffsetFromList(stat);
                        if (statOffsetFromRankConditional != 0f)
                        {
                            if (appendOverallRankText)
                            {
                                stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                                appendOverallRankText = false;
                            }
                            stringBuilder.AppendLine("    " + extraDecoration.Key.LabelCap + " (" + conditionalStat.Label + "): " + Core40kUtils.ValueToString(stat, statOffsetFromRankConditional, finalized: false, ToStringNumberSense.Offset));
                        }
                        var statFactorFromRankConditional = conditionalStat.statFactors.GetStatFactorFromList(stat);
                        if (statFactorFromRankConditional != 1f)
                        {
                            if (appendOverallRankText)
                            {
                                stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                                appendOverallRankText = false;
                            }
                            stringBuilder.AppendLine("    " + extraDecoration.Key.LabelCap + " (" + conditionalStat.Label + "): " + Core40kUtils.ValueToString(stat, statFactorFromRankConditional, finalized: false, ToStringNumberSense.Factor));
                        }
                    }
                }  
            }
        }
        
        //Weapon
        appendOverallRankText = true;
        if (cachedDecoratives.weapon != null)
        {
            var compWeapon = cachedDecoratives.weapon.GetComp<CompWeaponDecoration>();
            foreach (var weaponDecoration in compWeapon.WeaponDecorations)
            {
                var statOffsetFromRank = weaponDecoration.Key.statOffsets.GetStatOffsetFromList(stat);
                if (statOffsetFromRank != 0f)
                {
                    if (appendOverallRankText)
                    {
                        stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_WeaponDeco".Translate());
                        appendOverallRankText = false;
                    }
                    stringBuilder.AppendLine("    " + weaponDecoration.Key.LabelCap + ": " + Core40kUtils.ValueToString(stat, statOffsetFromRank, finalized: false, ToStringNumberSense.Offset));
                }
                var statFactorFromRank = weaponDecoration.Key.statFactors.GetStatFactorFromList(stat);
                if (statFactorFromRank != 1f)
                {
                    if (appendOverallRankText)
                    {
                        stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_WeaponDeco".Translate());
                        appendOverallRankText = false;
                    }
                    stringBuilder.AppendLine("    " + weaponDecoration.Key.LabelCap + ": " + Core40kUtils.ValueToString(stat, statFactorFromRank, finalized: false, ToStringNumberSense.Factor));
                }
                if (weaponDecoration.Key.conditionalStatAffecters.NullOrEmpty())
                {
                    continue;
                }

                foreach (var conditionalStat in weaponDecoration.Key.conditionalStatAffecters)
                {
                    if (!conditionalStat.Applies(req))
                    {
                        continue;
                    }
                    var statOffsetFromRankConditional = conditionalStat.statOffsets.GetStatOffsetFromList(stat);
                    if (statOffsetFromRankConditional != 0f)
                    {
                        if (appendOverallRankText)
                        {
                            stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                            appendOverallRankText = false;
                        }
                        stringBuilder.AppendLine("    " + weaponDecoration.Key.LabelCap + " (" + conditionalStat.Label + "): " + Core40kUtils.ValueToString(stat, statOffsetFromRankConditional, finalized: false, ToStringNumberSense.Offset));
                    }
                    var statFactorFromRankConditional = conditionalStat.statFactors.GetStatFactorFromList(stat);
                    if (statFactorFromRankConditional != 1f)
                    {
                        if (appendOverallRankText)
                        {
                            stringBuilder.AppendLine("BEWH.Framework.RankSystem.StatsReport_Rank".Translate());
                            appendOverallRankText = false;
                        }
                        stringBuilder.AppendLine("    " + weaponDecoration.Key.LabelCap + " (" + conditionalStat.Label + "): " + Core40kUtils.ValueToString(stat, statFactorFromRankConditional, finalized: false, ToStringNumberSense.Factor));
                    }
                }
            }
        }
        
        return stringBuilder;
    }
}