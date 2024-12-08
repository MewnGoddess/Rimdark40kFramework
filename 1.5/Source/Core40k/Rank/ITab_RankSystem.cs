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
        private RankInfoForTab currentlySelectedRank = null;
        
        private RankCategoryDef currentlySelectedRankCategory = null;
        
        private List<RankCategoryDef> availableCategories = new List<RankCategoryDef>();
        
        private List<RankInfoForTab> availableRanksForCategory = new List<RankInfoForTab>();

        private Pawn pawn;
        
        private CompRankInfo compRankInfo;
        
        Dictionary<RankDef, Vector2> rankPos = new Dictionary<RankDef, Vector2>();

        private Core40kModSettings modSettings;
        
        private GameComponent_RankInfo gameCompRankInfo;
        
        private static readonly CachedTexture LockedIcon = new CachedTexture("UI/Misc/LockedIcon");
        
        const float rankIconRectSize = 40f;
        const float rankIconGapSize = 40f;
        const float rankPlacementMult = rankIconGapSize + rankIconRectSize;
        const float rankIconMargin = 20f;

        private static readonly Color requirementMetColour = Color.white;
        private static readonly Color requirementNotMetColour = new Color(1f, 0.0f, 0.0f, 0.8f);
        
        private Vector2 scrollPosition;

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
            if (gameCompRankInfo == null)
            {
                gameCompRankInfo = Current.Game.GetComponent<GameComponent_RankInfo>();
            }
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
            currentlySelectedRank = availableRanksForCategory.FirstOrFallback(rank => rank.rankDef.defaultFirstRank, fallback: null);
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
            
            var categoryTextRect = new Rect(rect2)
            {
                height = 30f
            };
            categoryTextRect.width /= 2;
            categoryTextRect.x += categoryTextRect.width/2;

            curY += categoryTextRect.height;
            
            if (Prefs.DevMode)
            {
                const float width = 80f;
                const float padding = 30f;
                var debugResetRankRect = new Rect(categoryTextRect)
                {
                    width = width,
                };
                debugResetRankRect.height += 10f;
                debugResetRankRect.y += -5f;
                debugResetRankRect.x -= debugResetRankRect.width + padding;
                
                var debugUnlockRankRect = new Rect(categoryTextRect)
                {
                    width = width,
                };
                debugUnlockRankRect.height += 10f;
                debugUnlockRankRect.y += -5f;
                debugUnlockRankRect.x += categoryTextRect.width + padding;
                
                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(debugResetRankRect,"dev:\nreset ranks"))
                {
                    compRankInfo.ResetRanks(currentlySelectedRankCategory);
                }
                
                if (Widgets.ButtonText(debugUnlockRankRect,"dev:\nunlock rank"))
                {
                    compRankInfo.UnlockRank(currentlySelectedRank.rankDef);
                }
                Text.Font = GameFont.Medium;
            }

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
                if (!list.NullOrEmpty())
                {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
            
            var toolTip = currentlySelectedRankCategory != null ? currentlySelectedRankCategory.label.CapitalizeFirst() : "None";
            TooltipHandler.TipRegion(categoryTextRect, toolTip);

            curY += 12f;

            UpdateRanksForCategory();
            
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

        private (float yMax, float yMin, float xMax) GetYAndX()
        {
            var ranks = availableRanksForCategory.Select(rank => rank.rankDef).ToList();
            
            var xMax = ranks.MaxBy(rank => rank.displayPosition.x);
            var yMax = ranks.MaxBy(rank => rank.displayPosition.y);
            var yMin = ranks.MinBy(rank => rank.displayPosition.y);
                
            return (yMax.displayPosition.y, yMin.displayPosition.y, xMax.displayPosition.x);
        }
        
        private void FillRankTree(Rect rectRankTree)
        {
            var viewRect = new Rect(rectRankTree);

            viewRect.ContractedBy(20f);
            
            var yStart = viewRect.height / 2 + rankIconRectSize;
            var xStart = rectRankTree.xMin + rankIconMargin;
            
            var (yMax, yMin, xMax) = GetYAndX();
            
            var newYMin = Math.Abs(yMin * rankIconRectSize) + (Math.Abs(yMin * rankIconGapSize) - rankIconGapSize/2) + rankIconMargin;
            var newYMax = Math.Abs(yMax * rankIconRectSize) + (Math.Abs(yMax * rankIconGapSize) - rankIconGapSize/2) + rankIconMargin;
            var newXMax = Math.Abs(xMax * rankPlacementMult) + rankIconMargin * 2;

            if (newYMin - yStart > 0)
            {
                viewRect.yMin -= Math.Abs(newYMin);
            }
            if (newYMax - yStart > 0)
            {
                viewRect.yMax += Math.Abs(newYMax);
            }
            if (newXMax > viewRect.width)
            {
                viewRect.width = newXMax;
            }
            
            Widgets.BeginScrollView(rectRankTree.ContractedBy(10f), ref scrollPosition, viewRect);
            
            Widgets.DrawRectFast(viewRect, new Color(0f, 0f, 0f, 0.3f));
            
            //Find positions for ranks if they're not presents. Will only happen when category is switched
            if (rankPos.NullOrEmpty())
            {
                foreach (var rank in availableRanksForCategory)
                {
                    var rankRect = new Rect
                    {
                        width = rankIconRectSize,
                        height = rankIconRectSize,
                        x = xStart + rank.rankDef.displayPosition.x * rankPlacementMult,
                        y = yStart + rank.rankDef.displayPosition.y * rankPlacementMult,
                    };

                    if (rank.rankDef.displayPosition.x < 0)
                    {
                        Log.Error(rank.rankDef.defName + " has display position with x < 0. Should be 0 or above");
                    }

                    if (!rankPos.ContainsKey(rank.rankDef))
                    {
                        rankPos.Add(rank.rankDef, rankRect.position);
                    }
                }
            }

            //Draws requirement lines
            foreach (var rank in availableRanksForCategory)
            {
                if (rank.rankDef.rankRequirements == null)
                {
                    continue;
                }
                foreach (var rankReq in rank.rankDef.rankRequirements)
                {
                    var startPos = new Vector2(rankPos[rank.rankDef].x + rankIconRectSize/2, rankPos[rank.rankDef].y + rankIconRectSize/2);
                    var endPos = new Vector2(rankPos[rankReq.rankDef].x + rankIconRectSize/2, rankPos[rankReq.rankDef].y + rankIconRectSize/2);
                    
                    var rankUnlocked = compRankInfo.HasRank(rankReq.rankDef) ? Color.white : Color.grey;

                    if (currentlySelectedRank.rankDef == rankReq.rankDef)
                    {
                        rankUnlocked = new Color(0.0f, 0.5f, 1f, 0.9f);
                    }
                    
                    Widgets.DrawLine(startPos, endPos, rankUnlocked, 2f);
                }
            }
            
            //Draws icons
            foreach (var rank in availableRanksForCategory)
            {
                var rankRect = new Rect
                {
                    width = rankIconRectSize,
                    height = rankIconRectSize,
                    x = xStart + rank.rankDef.displayPosition.x * rankPlacementMult,
                    y = yStart + rank.rankDef.displayPosition.y * rankPlacementMult,
                };

                if (rank == currentlySelectedRank)
                {
                    Widgets.DrawStrongHighlight(rankRect.ExpandedBy(4f));
                }
                
                DrawIcon(rankRect, rank.rankDef.RankIcon, true);
                
                if (!AlreadyUnlocked(rank.rankDef))
                {
                    var colour = rank.requirementsMet ? new Color(0f, 0f, 0f, 0.55f) : new Color(0f, 0f, 0f, 0.9f);
                    Widgets.DrawRectFast(rankRect, colour);
                }
                
                if (rank.rankDef.incompatibleRanks != null)
                {
                    if (Enumerable.Any(rank.rankDef.incompatibleRanks, rankDef => compRankInfo.HasRank(rankDef)))
                    {
                        DrawIcon(rankRect, LockedIcon.Texture, false);
                    }
                }
                
                if (Widgets.ButtonInvisible(rankRect))
                {
                    currentlySelectedRank = rank;
                }

                TooltipHandler.TipRegion(rankRect, rank.rankDef.label.CapitalizeFirst());
            }
            
            Widgets.EndScrollView();
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
                var rankLabel = currentlySelectedRank.rankDef.label.CapitalizeFirst();
                listingRankInfo.Label(rankLabel);
                
                if (compRankInfo.DaysAsRank.TryGetValue(currentlySelectedRank.rankDef, out var daysSpentAs))
                {
                    listingRankInfo.Gap(5);
                    Text.Font = GameFont.Small;
                    listingRankInfo.Label("BEWH.DaysSinceRankGiven".Translate(daysSpentAs));
                    Text.Font = GameFont.Medium;
                }
                
                //Unlock button
                if (currentlySelectedRank.requirementsMet && !AlreadyUnlocked(currentlySelectedRank.rankDef))
                {   
                    listingRankInfo.Gap();
                    listingRankInfo.Indent(rectRankInfo.width * 0.25f);
                    if (listingRankInfo.ButtonText("BEWH.UnlockRank".Translate(), widthPct: 0.5f))
                    {
                        UnlockRank(currentlySelectedRank.rankDef);
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
                listingRankInfo.Label(currentlySelectedRank.rankDef.description);

                //Requirements
                listingRankInfo.Gap();
                Text.Font = GameFont.Medium;
                listingRankInfo.Label("BEWH.RankRequirements".Translate());
                Text.Font = GameFont.Small;
                var requirementText = currentlySelectedRank.rankText;
                listingRankInfo.Label(requirementText);
                
                //Given stats
                listingRankInfo.Gap();
                Text.Font = GameFont.Medium;
                listingRankInfo.Label("BEWH.RankBonuses".Translate());
                Text.Font = GameFont.Small;
                var rankBonusText = BuildRankBonusString(currentlySelectedRank.rankDef);
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
            var ranksForCategory = DefDatabase<RankDef>.AllDefsListForReading.Where(rank => rank.rankCategory == currentlySelectedRankCategory).ToList();

            foreach (var rank in ranksForCategory)
            {
                var rankInfo = BuildRankInfoForCategory(rank);
                availableRanksForCategory.Add(rankInfo);
            }
        }

        private void UpdateRanksForCategory()
        {
            foreach (var rank in availableRanksForCategory)
            {
                var rankInfoForTab = rank;
                UpdateRankInfoForCategory(ref rankInfoForTab);
            }

            if (currentlySelectedRank != null)
            {
                UpdateRankInfoForCategory(ref currentlySelectedRank);
            }
        }
        
        private RankInfoForTab BuildRankInfoForCategory(RankDef rankDef)
        {
            var res = RequirementMetAndText(rankDef);
            
            return new RankInfoForTab
            {
                rankDef = rankDef,
                requirementsMet = res.requirementMet,
                rankText = res.requirementText,
            };
        }

        private void UpdateRankInfoForCategory(ref RankInfoForTab rankInfoForTab)
        {
            var res = RequirementMetAndText(rankInfoForTab.rankDef);

            rankInfoForTab.requirementsMet = res.requirementMet;
            rankInfoForTab.rankText = res.requirementText;
        }

        private (bool requirementMet, string requirementText) RequirementMetAndText(RankDef rankDef)
        {
            var stringBuilder = new StringBuilder();
            
            //Limit on rank amount
            var rankLimitRequirementsMet = true;
            if (rankDef.colonyLimitOfRank.x > 0 || (rankDef.colonyLimitOfRank.x == 0 && rankDef.colonyLimitOfRank.y > 0))
            {
                var playerPawnAmount = GetColonistForCounting();
                
                var allowedAmount = rankDef.colonyLimitOfRank.y > 0 ? rankDef.colonyLimitOfRank.x + Math.Floor(playerPawnAmount/rankDef.colonyLimitOfRank.y) : rankDef.colonyLimitOfRank.x;
                
                var currentAmount = 0;

                if (gameCompRankInfo.rankLimits.ContainsKey(rankDef))
                {
                    currentAmount = gameCompRankInfo.rankLimits.TryGetValue(rankDef);
                }

                rankLimitRequirementsMet = allowedAmount > currentAmount;
                
                var requirementColour = rankLimitRequirementsMet ? requirementMetColour : requirementNotMetColour;
                
                stringBuilder.Append("\n");
                if (rankDef.colonyLimitOfRank.y == 0)
                {
                    stringBuilder.AppendLine("BEWH.RequirementsLimitOnlyOneEver".Translate(allowedAmount, currentAmount).Colorize(requirementColour));
                }
                else
                {
                    var limitIncreaseAmount = rankDef.colonyLimitOfRank.y;
                    var text = " " + limitIncreaseAmount;
                    switch (limitIncreaseAmount)
                    {
                        case 1:
                            text = "";
                            break;
                        case 2:
                            text += "nd";
                            break;
                        case 3:
                            text += "rd";
                            break;
                        default:
                            text += "th";
                            break;
                    }
                    stringBuilder.AppendLine("BEWH.RequirementsLimit".Translate(allowedAmount, currentAmount, text).Colorize(requirementColour));
                }
            }
            
            //Skills
            var skillRequirementsMet = true;
            if (!rankDef.requiredSkills.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsSkills".Translate());
                foreach (var aptitude in rankDef.requiredSkills)
                {
                    var skillRequirementMet = pawn.skills.GetSkill(aptitude.skill).Level >= aptitude.level;
                    if (!skillRequirementMet)
                    {
                        skillRequirementsMet = false;
                    }
                    var requirementColour = skillRequirementMet ? requirementMetColour : requirementNotMetColour;
                    stringBuilder.AppendLine(("    " + aptitude.skill.label.CapitalizeFirst() + ": " + aptitude.level).Colorize(requirementColour));
                }
            }
            
            //Ranks
            var rankRequirementsMet = true;
            if (!rankDef.rankRequirements.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsRanks".Translate());
                foreach (var rank in rankDef.rankRequirements)
                {
                    var rankRequirementMet = compRankInfo.HasRank(rank.rankDef) &&
                                                 compRankInfo.DaysAsRank[rank.rankDef] >= rank.daysAs;
                    
                    if (!rankRequirementMet)
                    {
                        rankRequirementsMet = false;
                    }
                    
                    var requirementColour = rankRequirementMet ? requirementMetColour : requirementNotMetColour;
                    
                    if (rank.daysAs > 0)
                    {
                        stringBuilder.AppendLine(("    " + "BEWH.HaveBeenRankForDays".Translate(rank.rankDef.label.CapitalizeFirst(), rank.daysAs)).Colorize(requirementColour));
                    }
                    else
                    {
                        stringBuilder.AppendLine(("    " + "BEWH.HaveAchievedRank".Translate(rank.rankDef.label.CapitalizeFirst())).Colorize(requirementColour));
                    }
                    
                }
            }
            
            //Traits
            //All
            var traitsAllRequirementsMet = true;
            if (!rankDef.requiredTraitsAll.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsTraitAll".Translate());
                foreach (var trait in rankDef.requiredTraitsAll)
                {
                    var traitsAllRequirementMet = pawn.story.traits.HasTrait(trait.traitDef, trait.degree);
                    
                    if (!traitsAllRequirementMet)
                    {
                        traitsAllRequirementsMet = false;
                    }
                    
                    var requirementColour = traitsAllRequirementMet ? requirementMetColour : requirementNotMetColour;
                    stringBuilder.AppendLine(("    " + trait.traitDef.DataAtDegree(trait.degree).label.CapitalizeFirst()).Colorize(requirementColour));
                }
            }
            //One Among
            var traitsAtLeastOneRequirementsMet = rankDef.requiredTraitsOneAmong.NullOrEmpty();
            if (!rankDef.requiredTraitsOneAmong.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.RequirementsTraitAtLeastOne".Translate());
                foreach (var trait in rankDef.requiredTraitsOneAmong)
                {
                    var traitsAtLeastOneRequirementMet = pawn.story.traits.HasTrait(trait.traitDef, trait.degree);
                        
                    if (traitsAtLeastOneRequirementMet)
                    {
                        traitsAtLeastOneRequirementsMet = true;
                    }
                    
                    var requirementColour = traitsAtLeastOneRequirementMet ? requirementMetColour : requirementNotMetColour;
                    
                    stringBuilder.AppendLine(("    " + trait.traitDef.DataAtDegree(trait.degree).label.CapitalizeFirst()).Colorize(requirementColour));
                }
            }
            
            //Genes
            var genesAllRequirementsMet = true;
            var genesAtLeastOneRequirementsMet = rankDef.requiredGenesOneAmong.NullOrEmpty();
            if (pawn.genes != null)
            {
                //All
                if (!rankDef.requiredGenesAll.NullOrEmpty())
                {
                    stringBuilder.Append("\n");
                    stringBuilder.AppendLine("BEWH.RequirementsGeneAll".Translate());
                    foreach (var gene in rankDef.requiredGenesAll)
                    {
                        var genesAllRequirementMet = pawn.genes.HasActiveGene(gene);
                        
                        if (!genesAllRequirementMet)
                        {
                            genesAllRequirementsMet = false;
                        }
                        
                        var requirementColour = genesAllRequirementMet ? requirementMetColour : requirementNotMetColour;
                        
                        stringBuilder.AppendLine(("    " + gene.label.CapitalizeFirst()).Colorize(requirementColour));
                    }
                }
                //One Among
                if (!rankDef.requiredGenesOneAmong.NullOrEmpty())
                {
                    stringBuilder.Append("\n");
                    stringBuilder.AppendLine("BEWH.RequirementsGeneAtLeastOne".Translate());
                    foreach (var gene in rankDef.requiredGenesOneAmong)
                    {
                        var genesAtLeastOneRequirementMet = pawn.genes.HasActiveGene(gene);
                        
                        if (genesAtLeastOneRequirementMet)
                        {
                            genesAtLeastOneRequirementsMet = true;
                        }
                    
                        var requirementColour = genesAtLeastOneRequirementMet ? requirementMetColour : requirementNotMetColour;
                        stringBuilder.AppendLine(("    " + gene.label.CapitalizeFirst()).Colorize(requirementColour));
                    }
                }
            }
            //Incompatible Ranks
            var noIncompatibleRanks = true;
            if (!rankDef.incompatibleRanks.NullOrEmpty())
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.IncompatibleRank".Translate());
                foreach (var rank in rankDef.incompatibleRanks)
                {
                    var isIncompatibleRank = compRankInfo.HasRank(rank);

                    if (isIncompatibleRank)
                    {
                        noIncompatibleRanks = false;
                    }
                    
                    var requirementColour = !isIncompatibleRank ? requirementMetColour : requirementNotMetColour;
                    
                    stringBuilder.AppendLine(("    " + rank.label.CapitalizeFirst()).Colorize(requirementColour));
                }
            }
            
            var requirementText = stringBuilder.ToString().TrimEnd('\r', '\n').TrimStart('\r', '\n');
            if (requirementText.NullOrEmpty())
            {
                requirementText = "    " + "BEWH.None".Translate();
            }

            var requirementsMet = skillRequirementsMet && rankRequirementsMet &&
                                             rankLimitRequirementsMet && noIncompatibleRanks &&
                                             traitsAllRequirementsMet && traitsAtLeastOneRequirementsMet &&
                                             genesAllRequirementsMet && genesAtLeastOneRequirementsMet;
            
            return (requirementsMet, requirementText);
        }
        
        private static string BuildRankBonusString(RankDef rankDef)
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
            
            
            var customEffectStringBuilder = new StringBuilder();

            foreach (var customEffect in rankDef.customEffectDescriptions)
            {
                customEffectStringBuilder.Append("    " + customEffect.CapitalizeFirst());
            }

            var customEffects = customEffectStringBuilder.ToString();
            if (!customEffects.NullOrEmpty())
            {
                customEffects = "BEWH.OtherEffects".Translate() + "\n" + customEffects;
            }

            var result = statBonuses + "\n" + abilityBonuses + "\n" + customEffects;
            
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
            return compRankInfo != null && compRankInfo.HasRank(rankDef);
        }

        private static void DrawIcon(Rect inRect, Texture2D icon, bool drawBg)
        {
            var color = Mouse.IsOver(inRect) ? GenUI.MouseoverColor : Color.white;
            GUI.color = color;
            if (drawBg)
            {
                GUI.DrawTexture(inRect, Command.BGTexShrunk);
            }
            GUI.color = Color.white;
            GUI.DrawTexture(inRect, icon);
        }
        
        private void UnlockRank(RankDef rank)
        {
            compRankInfo.UnlockRank(rank);
        }
    }

    internal class RankInfoForTab
    {
        public RankDef rankDef;
        public bool requirementsMet;
        public string rankText;
    }
}