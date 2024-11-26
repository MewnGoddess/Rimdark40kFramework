using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.UItils;

namespace Core40k
{
    [StaticConstructorOnStartup]
    public class ITab_RankSystem : ITab
    {
        private RankDef currentlySelectedRank = null;
        
        private RankCategoryDef currentlySelectedRankCategory = null;
        
        private List<RankCategoryDef> availableCategories = new List<RankCategoryDef>();
        
        private List<RankDef> availableRanksForCategory = new List<RankDef>();

        private Pawn pawn;
        
        private CompRankInfo compRankInfo;
        
        Dictionary<RankDef, Vector2> rankPos = new Dictionary<RankDef, Vector2>();

        private Core40kModSettings modSettings;
        
        private GameComponent_RankInfo gameCompRankInfo;
        
        const float rankIconRectSize = 40f;
        const float rankIconGapSize = 40f;
        const float rankPlacementMult = rankIconGapSize + rankIconRectSize;
        
        
        public override bool IsVisible
        {
            get
            {
                var defaultRes = modSettings.alwaysShowRankTab;
                if (!(Find.Selector.SingleSelectedThing is Pawn p) || !p.HasComp<CompRankInfo>() || p.Faction == null || !p.Faction.IsPlayer)
                {
                    return defaultRes;
                }

                return defaultRes || !availableCategories.NullOrEmpty();
            }
        }

        public ITab_RankSystem()
        {
            size = new Vector2(UI.screenWidth, UI.screenHeight * 0.75f);
            labelKey = "BEWH.RankTab";
            modSettings = LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();
            UpdateRankCategoryList();
        }
        
        public override void OnOpen()
        {
            base.OnOpen();
            pawn = (Pawn)Find.Selector.SingleSelectedThing;
            compRankInfo = pawn.GetComp<CompRankInfo>();
            rankPos.Clear();
            UpdateRankCategoryList();
            if (compRankInfo.LastOpenedRankCategory != null)
            {
                currentlySelectedRankCategory = compRankInfo.LastOpenedRankCategory;
            }
            else
            {
                currentlySelectedRankCategory = availableCategories.Count > 0 ? availableCategories[0] : null;
            }
            GetRanksForCategory();
            currentlySelectedRank = availableRanksForCategory.FirstOrFallback(rank => rank.defaultFirstRank, fallback: null);
            if (gameCompRankInfo == null)
            {
                gameCompRankInfo = Current.Game.GetComponent<GameComponent_RankInfo>();
            }
        }

        protected override void FillTab()   
        {
            if (pawn != SelPawn)
            {
                pawn = SelPawn;
                compRankInfo = pawn.GetComp<CompRankInfo>();
            }
            
            var font = Text.Font;
            var anchor = Text.Anchor;
            
            var rect = new Rect(Vector2.one * 20f, size - Vector2.one * 40f);
            var rect2 = rect.TakeLeftPart(size.x * 0.25f);
            
            var curY = rect.y;
            
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            
            //Button to switch between rank categories
            var categoryText = "BEWH.NoCategorySelected".Translate();
            if (currentlySelectedRankCategory != null)
            {
                categoryText = currentlySelectedRankCategory.label.CapitalizeFirst();
            }
            
            var categoryTextRect = new Rect(rect2);
            categoryTextRect.height = 30f;
            categoryTextRect.width /= 2;
            categoryTextRect.x += categoryTextRect.width/2;

            curY += categoryTextRect.height;

            if (Widgets.ButtonText(categoryTextRect,categoryText))
            {
                var list = new List<FloatMenuOption>();
                foreach (var category in availableCategories)
                {
                    if (currentlySelectedRankCategory == category)
                    {
                        continue;
                    }   
                    var menuOption = new FloatMenuOption(category.label.CapitalizeFirst(), delegate
                    {
                        currentlySelectedRankCategory = category;
                        compRankInfo.OpenedRankCategory(category);
                        currentlySelectedRank = null;
                        GetRanksForCategory();
                        rankPos.Clear();
                    }, Widgets.PlaceholderIconTex, Color.white);
                    if (!MeetsCategoryRequirements(category))
                    {
                        var newLabel = "BEWH.CategoryRequires".Translate(category.label.CapitalizeFirst(), DoesNotMeetRequirementTextForCategory(category));
                        menuOption.Disabled = true;
                        menuOption.Label = newLabel;
                    }
                    list.Add(menuOption);
                }
                //Does not show properly for some reason
                if (!list.NullOrEmpty())
                {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
            
            TooltipHandler.TipRegion(categoryTextRect, currentlySelectedRankCategory.description.CapitalizeFirst());

            curY += 12f;
            
            //Rank info
            var rectRankInfo = new Rect(rect2);
            rectRankInfo.height -= curY;
            rectRankInfo.y = curY;
            rectRankInfo.ContractedBy(10f);

            Widgets.DrawMenuSection(rectRankInfo);
            
            FillRankInfo(rectRankInfo);
            
            //Rank tree
            var rectRankTree = new Rect(rect)
            {
                xMin = rectRankInfo.xMax,
                yMin = rectRankInfo.yMin,
                yMax = rectRankInfo.yMax,
            };
            rectRankTree.xMin += 50f;

            Widgets.DrawMenuSection(rectRankTree);
            FillRankTree(rectRankTree);
            
            Text.Font = font;
            Text.Anchor = anchor;
        }

        private void FillRankTree(Rect rectRankTree)
        {
            var yStart = rectRankTree.height / 2 + rankIconRectSize / 2;
            var xStart = rectRankTree.xMin + 20f;
            
            //Find positions for ranks if they're not presents. Will only happend when category is switched
            if (rankPos.NullOrEmpty())
            {
                foreach (var rank in availableRanksForCategory)
                {
                    var rankRect = new Rect(rectRankTree)
                    {
                        width = rankIconRectSize,
                        height = rankIconRectSize,
                        x = xStart + rank.displayPosition.x * rankPlacementMult,
                        y = yStart + rank.displayPosition.y * rankPlacementMult,
                    };

                    if (rank.displayPosition.x < 0)
                    {
                        Log.Error(rank.defName + " has display position with x < 0. Should be 0 or above");
                    }
                
                    rankPos.Add(rank, rankRect.position);
                }
            }

            //Draws requirement lines
            foreach (var rank in availableRanksForCategory)
            {
                if (rank.rankRequirements == null)
                {
                    continue;
                }
                foreach (var rankReq in rank.rankRequirements)
                {
                    var startPos = new Vector2(rankPos[rank].x + rankIconRectSize/2, rankPos[rank].y + rankIconRectSize/2);
                    var endPos = new Vector2(rankPos[rankReq].x + rankIconRectSize/2, rankPos[rankReq].y + rankIconRectSize/2);
                    Widgets.DrawLine(startPos, endPos, compRankInfo.UnlockedRanks.Contains(rank) ? Color.white : Color.grey, 2f);
                }
            }
            
            //Draws icons
            foreach (var rank in availableRanksForCategory)
            {
                var rankRect = new Rect(rectRankTree)
                {
                    width = rankIconRectSize,
                    height = rankIconRectSize,
                    x = xStart + rank.displayPosition.x * rankPlacementMult,
                    y = yStart + rank.displayPosition.y * rankPlacementMult,
                };
                
                if (MeetsRankRequirements(rank) && !AlreadyUnlocked(rank))
                {
                    Widgets.DrawStrongHighlight(rankRect.ExpandedBy(6f));
                }
                
                DrawIcon(rankRect, rank);
                
                if (!MeetsRankRequirements(rank) && !AlreadyUnlocked(rank))
                {
                    Widgets.DrawRectFast(rankRect, new Color(0f, 0f, 0f, 0.6f));
                }
                
                if (Widgets.ButtonInvisible(rankRect))
                {
                    currentlySelectedRank = rank;
                }

                TooltipHandler.TipRegion(rankRect, rank.label.CapitalizeFirst());
            }
        }
        
        private void FillRankInfo(Rect rect)
        {
            var rectRankInfo = new Rect(rect);
            rectRankInfo.ContractedBy(20f);
            if (currentlySelectedRank != null)
            {
                var listingRankInfo = new Listing_Standard();
                //Start
                listingRankInfo.Begin(rectRankInfo);
                
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.UpperCenter;
                
                //Name
                listingRankInfo.Gap(5);
                var rankLabel = currentlySelectedRank.label.CapitalizeFirst();
                listingRankInfo.Label(rankLabel);
                
                //Unlock button
                if (MeetsRankRequirements(currentlySelectedRank) && !AlreadyUnlocked(currentlySelectedRank))
                {   
                    listingRankInfo.Gap();
                    listingRankInfo.Indent(rectRankInfo.width * 0.25f);
                    if (listingRankInfo.ButtonText("Unlock", widthPct: 0.5f))
                    {
                        UnlockRank(currentlySelectedRank);
                    }
                    listingRankInfo.Outdent(rectRankInfo.width * 0.25f);
                }
                
                listingRankInfo.GapLine(1f);
                listingRankInfo.Indent(rectRankInfo.width * 0.02f);
                
                //Description
                listingRankInfo.Gap();
                Text.Anchor = TextAnchor.UpperLeft;
                listingRankInfo.Label("BEWH.RankDescription".Translate());
                Text.Font = GameFont.Small;
                listingRankInfo.Label(currentlySelectedRank.description);

                //Requirements
                listingRankInfo.Gap();
                Text.Font = GameFont.Medium;
                listingRankInfo.Label("BEWH.RankRequirements".Translate());
                Text.Font = GameFont.Small;
                var requirementText = BuildRequirementString(currentlySelectedRank);
                listingRankInfo.Label(requirementText);
                
                //Given stats
                listingRankInfo.Gap();
                Text.Font = GameFont.Medium;
                listingRankInfo.Label("BEWH.RankBonuses".Translate());
                Text.Font = GameFont.Small;
                var rankBonusText = BuildRankBonusString(currentlySelectedRank);
                listingRankInfo.Label(rankBonusText);
                
                //End
                listingRankInfo.Outdent(rectRankInfo.width * 0.02f);
                listingRankInfo.End();
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                var text = "BEWH.NoRankSelected".Translate();
                Widgets.Label(rectRankInfo, text);
            }
        }

        private void UpdateRankCategoryList()
        {
            availableCategories = DefDatabase<RankCategoryDef>.AllDefsListForReading;
        }

        private void GetRanksForCategory()
        {
            availableRanksForCategory = DefDatabase<RankDef>.AllDefsListForReading.Where(rank => rank.rankCategory == currentlySelectedRankCategory).ToList();
        }

        private string BuildRequirementString(RankDef rankDef)
        {
            var stringBuilder = new StringBuilder();
            //var firstAppend = true;
            //Skills
            if (!rankDef.requiredSkills.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsSkills".Translate());
                foreach (var skill in rankDef.requiredSkills)
                {
                    stringBuilder.AppendLine("    " + skill.skill.label.CapitalizeFirst() + ": " + skill.level);
                }
            }
            //Ranks
            if (!rankDef.rankRequirements.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsRanks".Translate());
                foreach (var rank in rankDef.rankRequirements)
                {
                    stringBuilder.AppendLine("    " + rank.label.CapitalizeFirst());
                }
            }
            //Traits all
            if (!rankDef.requiredTraitsAll.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsTraitAll".Translate());
                foreach (var rank in rankDef.requiredTraitsAll)
                {
                    stringBuilder.AppendLine("    " + rank.traitDef.DataAtDegree(rank.degree).label.CapitalizeFirst());
                }
            }
            //Traits one among
            if (!rankDef.requiredTraitsOneAmong.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsTraitAtLeastOne".Translate());
                foreach (var rank in rankDef.requiredTraitsOneAmong)
                {
                    stringBuilder.AppendLine("    " + rank.traitDef.DataAtDegree(rank.degree).label.CapitalizeFirst());
                }
            }
            //Genes all
            if (!rankDef.requiredGenesAll.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsGeneAll".Translate());
                foreach (var rank in rankDef.requiredGenesAll)
                {
                    stringBuilder.AppendLine("    " + rank.label.CapitalizeFirst());
                }
            }
            //Genes one among
            if (!rankDef.requiredGenesOneAmong.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsGeneAtLeastOne".Translate());
                foreach (var rank in rankDef.requiredGenesOneAmong)
                {
                    stringBuilder.AppendLine("    " + rank.label.CapitalizeFirst());
                }
            }

            //Rank limits
            if (rankDef.colonyLimitOfRank.x > 0 || (rankDef.colonyLimitOfRank.x == 0 && rankDef.colonyLimitOfRank.y > 0))
            {
                var playerPawnAmount = GetColonistForCounting();
                
                var allowedAmount = rankDef.colonyLimitOfRank.y > 0 ? rankDef.colonyLimitOfRank.x + Math.Floor(playerPawnAmount/rankDef.colonyLimitOfRank.y) : rankDef.colonyLimitOfRank.x;
                
                var currentAmount = 0;

                if (gameCompRankInfo.rankLimits.ContainsKey(rankDef))
                {
                    currentAmount = gameCompRankInfo.rankLimits.TryGetValue(rankDef);
                }
                
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsLimit".Translate(allowedAmount, currentAmount));
            }
            
            var requirements = stringBuilder.ToString().TrimEnd('\r', '\n').TrimStart('\r', '\n');
            if (requirements.NullOrEmpty())
            {
                requirements = "    " + "BEWH.None".Translate();
            }
            
            return requirements;
        }

        private string BuildRankBonusString(RankDef rankDef)
        {
            var statStringBuilder = new StringBuilder();

            foreach (var statOffset in rankDef.statOffsets)
            {
                statStringBuilder.AppendLine("    " + statOffset.stat.label.CapitalizeFirst() + ": " + ValueToString(statOffset.stat, statOffset.value, finalized: false, ToStringNumberSense.Offset));
            }
            
            foreach (var statFactor in rankDef.statFactors)
            {
                statStringBuilder.AppendLine("    " + statFactor.stat.label.CapitalizeFirst() + ": " + ValueToString(statFactor.stat, statFactor.value, finalized: false, ToStringNumberSense.Factor));
            }
            
            var statBonuses = statStringBuilder.ToString();
            if (!statBonuses.NullOrEmpty())
            {
                statBonuses = "BEWH.Stats".Translate() + "\n" + statBonuses;
            }
            
            var abilityStringBuilder = new StringBuilder();
            
            foreach (var ability in rankDef.givesAbilities)
            {
                abilityStringBuilder.AppendLine("    " + ability.label.CapitalizeFirst());
            }
            
            foreach (var abilityVfe in rankDef.givesVFEAbilities)
            {
                abilityStringBuilder.AppendLine("    " + abilityVfe.label.CapitalizeFirst());
            }
            
            var abilityBonuses = abilityStringBuilder.ToString();
            
            if (!abilityBonuses.NullOrEmpty())
            {
                abilityBonuses = "BEWH.Abilities".Translate() + "\n" + abilityBonuses;
            }

            var result = statBonuses + "\n" + abilityBonuses;
            
            if (statBonuses.NullOrEmpty() && abilityBonuses.NullOrEmpty())
            {
                result = "    " + "BEWH.None".Translate();
            }

            return result;
        }
        
        private static string ValueToString(StatDef stat, float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
        {
            if (!finalized)
            {
                var text = val.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense);
                if (numberSense != ToStringNumberSense.Factor && !stat.formatStringUnfinalized.NullOrEmpty())
                {
                    text = string.Format(stat.formatStringUnfinalized, text);
                }
                return text;
            }
            var text2 = val.ToStringByStyle(stat.toStringStyle, numberSense);
            if (numberSense != ToStringNumberSense.Factor && !stat.formatString.NullOrEmpty())
            {
                text2 = string.Format(stat.formatString, text2);
            }
            return text2;
        }
        
        private bool MeetsRankRequirements(RankDef rankDef)
        {
            //Skills
            var skillRequirementMet = true;
            if (!rankDef.requiredSkills.NullOrEmpty())
            {
                skillRequirementMet = rankDef.requiredSkills.All(skill => pawn.skills.GetSkill(skill.skill).Level >= skill.level);
            }
            
            //Ranks
            var rankRequirementMet = true;
            if (!rankDef.rankRequirements.NullOrEmpty())
            {
                rankRequirementMet = rankDef.rankRequirements.All(rank => compRankInfo.UnlockedRanks.Contains(rank));
            }

            //Traits
            var traitsAllRequirementMet = true;
            if (!rankDef.requiredTraitsAll.NullOrEmpty())
            {
                traitsAllRequirementMet = rankDef.requiredTraitsAll.All(trait => pawn.story.traits.HasTrait(trait.traitDef, trait.degree));
            }
            var traitsAtLeastOneRequirementMet = true;
            if (!rankDef.requiredTraitsOneAmong.NullOrEmpty())
            {
                traitsAtLeastOneRequirementMet = rankDef.requiredTraitsOneAmong.Any(trait => pawn.story.traits.HasTrait(trait.traitDef, trait.degree));
            }

            //Genes
            var genesAllRequirementMet = true;
            var genesAtLeastOneRequirementMet = true;
            if (pawn.genes != null)
            {
                if (!rankDef.requiredGenesAll.NullOrEmpty())
                {
                    genesAllRequirementMet = rankDef.requiredGenesAll.All(gene => pawn.genes.HasActiveGene(gene));
                }
                if (!rankDef.requiredGenesOneAmong.NullOrEmpty())
                {
                    genesAtLeastOneRequirementMet = rankDef.requiredGenesOneAmong.Any(gene => pawn.genes.HasActiveGene(gene));
                }
            }
            
            //Limit on rank amount
            var rankLimitRequirementMet = true;
            if (rankDef.colonyLimitOfRank.x > 0 || (rankDef.colonyLimitOfRank.x == 0 && rankDef.colonyLimitOfRank.y > 0))
            {
                var playerPawnAmount = GetColonistForCounting();
                
                var allowedAmount = rankDef.colonyLimitOfRank.y > 0 ? rankDef.colonyLimitOfRank.x + Math.Floor(playerPawnAmount/rankDef.colonyLimitOfRank.y) : rankDef.colonyLimitOfRank.x;
                
                var currentAmount = 0;

                if (gameCompRankInfo.rankLimits.ContainsKey(rankDef))
                {
                    currentAmount = gameCompRankInfo.rankLimits.TryGetValue(rankDef);
                }

                rankLimitRequirementMet = allowedAmount > currentAmount;
                
                //And whenever they die, harmony patch to remove if needed. patch both them dying and resurrection utility.
            }
            
            return skillRequirementMet && rankRequirementMet && traitsAllRequirementMet && traitsAtLeastOneRequirementMet && genesAllRequirementMet && genesAtLeastOneRequirementMet && rankLimitRequirementMet;
        }

        private static int GetColonistForCounting()
        {
            var playerPawnAmount = Find.Maps.Sum(map => map.mapPawns.ColonistCount);
            var caravans = Find.WorldObjects.Caravans.Where(c => c.IsPlayerControlled);
            playerPawnAmount += caravans.SelectMany<Caravan, Pawn>(caravan => caravan.pawns).Count(p => p.Faction != null && p.Faction.IsPlayer);

            return playerPawnAmount;
        }

        private bool MeetsCategoryRequirements(RankCategoryDef rankCategoryDef)
        {
            var geneRequirementMet = true;
            if (pawn.genes != null && rankCategoryDef.unlockedByGene != null)
            {
                geneRequirementMet = pawn.genes.HasActiveGene(rankCategoryDef.unlockedByGene);
            }
            
            var hediffRequirementMet = true;
            if (rankCategoryDef.unlockedByHediff != null)
            {
                hediffRequirementMet = pawn.health.hediffSet.HasHediff(rankCategoryDef.unlockedByHediff);
            }
            
            var traitRequirementMet = true;
            if (rankCategoryDef.unlockedByTrait != null)
            {
                traitRequirementMet = pawn.story.traits.HasTrait(rankCategoryDef.unlockedByTrait, rankCategoryDef.traitDegree);
            }

            return geneRequirementMet && hediffRequirementMet && traitRequirementMet;
        }

        private string DoesNotMeetRequirementTextForCategory(RankCategoryDef rankCategoryDef)
        {
            var stringBuilder = new StringBuilder();
            var allRequirement = new List<string>();
            if (pawn.genes != null && rankCategoryDef.unlockedByGene != null && !pawn.genes.HasActiveGene(rankCategoryDef.unlockedByGene))
            {
                allRequirement.Add("BEWH.CategoryRequiredGene".Translate(rankCategoryDef.unlockedByGene.label.CapitalizeFirst()));
            }
            
            if (rankCategoryDef.unlockedByHediff != null && !pawn.health.hediffSet.HasHediff(rankCategoryDef.unlockedByHediff))
            {
                allRequirement.Add("BEWH.CategoryRequiredHediff".Translate(rankCategoryDef.unlockedByHediff.label.CapitalizeFirst()));
            }
            
            if (rankCategoryDef.unlockedByTrait != null && !pawn.story.traits.HasTrait(rankCategoryDef.unlockedByTrait, rankCategoryDef.traitDegree))
            {
                allRequirement.Add("BEWH.CategoryRequiredTrait".Translate(rankCategoryDef.unlockedByTrait.DataAtDegree(rankCategoryDef.traitDegree).label.CapitalizeFirst()));
            }

            for (var i = 0; i < allRequirement.Count; i++)
            {
                stringBuilder.Append(allRequirement[i]);
                if (i + 2 == allRequirement.Count)
                {
                    stringBuilder.Append("BEWH.And".Translate());
                }
                else if (i + 2 < allRequirement.Count)
                {
                    stringBuilder.Append(", ");
                }
            }

            return stringBuilder.ToString();
        }
        
        private bool AlreadyUnlocked(RankDef rankDef)
        {
            return compRankInfo != null && compRankInfo.UnlockedRanks.Contains(rankDef);
        }

        private static void DrawIcon(Rect inRect, RankDef rankDef)
        {
            var color = Mouse.IsOver(inRect) ? GenUI.MouseoverColor : Color.white;
            GUI.color = color;
            GUI.DrawTexture(inRect, Command.BGTexShrunk);
            GUI.color = Color.white;
            GUI.DrawTexture(inRect, rankDef.RankIcon);
        }
        
        private void UnlockRank(RankDef rank)
        {
            compRankInfo.UnlockRank(rank);
            if (rank.givesAbilities != null)
            {
                foreach (var ability in rank.givesAbilities)
                {
                    pawn.abilities.GainAbility(ability);
                }
            }
            
            if (rank.givesVFEAbilities != null)
            {
                var comp = pawn.GetComp<CompAbilities>();
                if (comp != null)
                {
                    foreach (var ability in rank.givesVFEAbilities)
                    {
                        comp.GiveAbility(ability);
                    }
                    
                }
            }

            if (gameCompRankInfo.rankLimits.ContainsKey(rank))
            {
                gameCompRankInfo.rankLimits[rank] += 1;
            }
            else
            {
                gameCompRankInfo.rankLimits.Add(rank, 1);
            }
        }
    }
}