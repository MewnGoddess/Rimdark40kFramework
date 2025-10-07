﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using VEF.Utils;
using Verse;

namespace Core40k;

[StaticConstructorOnStartup]
public class ITab_RankSystem : ITab
{
    private RankInfoForTab currentlySelectedRank = null;
        
    private RankCategoryDef currentlySelectedRankCategory = null;
        
    private List<RankCategoryDef> availableCategories = new List<RankCategoryDef>();
        
    private List<RankInfoForTab> availableRanksForCategory = new List<RankInfoForTab>();

    private Pawn pawn;
        
    private CompRankInfo compRankInfo;

    private bool redoRankInfo = false;
    
    Dictionary<RankDef, Vector2> rankPos = new Dictionary<RankDef, Vector2>();

    private Core40kModSettings modSettings;
    private Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();
        
    private GameComponent_RankInfo gameCompRankInfo;
        
    private static readonly CachedTexture LockedIcon = new CachedTexture("UI/Misc/LockedIcon");
        
    const float rankIconRectSize = 40f;
    const float rankIconGapSize = 40f;
    const float rankPlacementMult = rankIconGapSize + rankIconRectSize;
    const float rankIconMargin = 20f;

    private static readonly Color requirementMetColour = Color.white;
    private static readonly Color requirementNotMetColour = new Color(1f, 0.0f, 0.0f, 0.8f);

    public override bool IsVisible
    {
        get
        {
            if (SelPawn == null)
            {
                return false;
            }
            
            var defaultRes = ModSettings.alwaysShowRankTab;
            if (!(Find.Selector.SingleSelectedThing is Pawn p) || !p.HasComp<CompRankInfo>() || p.Faction == null || !p.Faction.IsPlayer || p.IsSlaveOfColony || p.IsPrisonerOfColony)
            {
                return defaultRes;
            }
            
            foreach (var rankCategoryDef in availableCategories)
            {
                if (rankCategoryDef.RankCategoryUnlockedFor(SelPawn))
                {
                    return true;
                }
            }

            return defaultRes;
        }
    }

    public ITab_RankSystem()
    {
        labelKey = "BEWH.Framework.RankSystem.RankTab";
        UpdateRankCategoryList();
    }
        
    public override void OnOpen()
    {
        base.OnOpen();
        pawn = SelPawn;

        size = new Vector2(UI.screenWidth, PaneTopY - 100);
        
        compRankInfo = pawn.GetComp<CompRankInfo>();
        if (compRankInfo == null)
        {
            CloseTab();
            return;
        }
        rankPos.Clear();
        gameCompRankInfo ??= Current.Game.GetComponent<GameComponent_RankInfo>();
        UpdateRankCategoryList();
        if (compRankInfo.LastOpenedRankCategory != null && compRankInfo.LastOpenedRankCategory.RankCategoryUnlockedFor(SelPawn))
        {
            currentlySelectedRankCategory = compRankInfo.LastOpenedRankCategory;
        }
        else
        {
            currentlySelectedRankCategory = null;
            foreach (var availableCategory in availableCategories.Where(availableCategory => availableCategory.RankCategoryUnlockedFor(pawn)))
            {
                currentlySelectedRankCategory = availableCategory;
            }
        }
        GetRanksForCategory();
        if (!compRankInfo.UnlockedRanks.NullOrEmpty())
        {
            var highestRank = compRankInfo.HighestRankDef(true) ?? compRankInfo.HighestRankDef(false);
            currentlySelectedRank = availableRanksForCategory.FirstOrFallback(rank => rank.rankDef == highestRank, fallback: null);
        }
        else
        {
            currentlySelectedRank = availableRanksForCategory.FirstOrFallback(rank => rank.rankDef.defaultFirstRank, fallback: null);
        }
    }

    protected override void FillTab()
    {
        if (pawn != SelPawn)
        {
            CloseTab();
            return;
        }

        var font = Text.Font;
        var anchor = Text.Anchor;
            
        var rect = new Rect(Vector2.one * 20f, size - Vector2.one * 40f);
        var rect2 = rect.TakeLeftPart(size.x * 0.25f);
            
        var curY = rect.y;
            
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleCenter;
            
        //Button to switch between rank categories
        var categoryText = "BEWH.Framework.RankSystem.NoCategorySelected".Translate();
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
            
        //Dev Options
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
                UnlockRank(currentlySelectedRank.rankDef);
            }
            Text.Font = GameFont.Medium;
        }

        //Select rank category
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
                if (!category.RankCategoryUnlockedFor(pawn))
                {
                    var newLabel = category.RankCategoryRequirementsNotMetFor(pawn);
                    menuOption.Disabled = true;
                    menuOption.tooltip = newLabel;
                }
                list.Add(menuOption);
            }

            if (list.NullOrEmpty())
            {
                var menuOptionNone = new FloatMenuOption("NoneBrackets".Translate(), null);
                list.Add(menuOptionNone);
            }
            
            Find.WindowStack.Add(new FloatMenu(list));
        }
            
        var toolTip = currentlySelectedRankCategory != null ? currentlySelectedRankCategory.label.CapitalizeFirst() : "BEWH.Framework.CommonKeyword.None".Translate().ToString();
        TooltipHandler.TipRegion(categoryTextRect, toolTip);

        curY += 12f;

        if (redoRankInfo)
        {
            GetRanksForCategory();
            redoRankInfo = false;
        }
            
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
            
        var xMax = ranks.MaxBy(rank => currentlySelectedRankCategory.rankDict[rank].displayPosition.x);
        var yMax = ranks.MaxBy(rank => currentlySelectedRankCategory.rankDict[rank].displayPosition.y);
        var yMin = ranks.MinBy(rank => currentlySelectedRankCategory.rankDict[rank].displayPosition.y);
                
        return (currentlySelectedRankCategory.rankDict[yMax].displayPosition.y, currentlySelectedRankCategory.rankDict[yMin].displayPosition.y, currentlySelectedRankCategory.rankDict[xMax].displayPosition.x);
    }
        
    private Vector2 scrollPosition;
    private void FillRankTree(Rect rectRankTree)
    {
        if (currentlySelectedRankCategory == null)
        {
            return;
        }
        
        var viewRect = new Rect(rectRankTree);

        viewRect.ContractedBy(20f);
            
        var yStart = viewRect.height / 2 + rankIconRectSize;
        var xStart = rectRankTree.xMin + rankIconMargin;
            
        var (yMax, yMin, xMax) = GetYAndX();
            
        //Needs correction on larger uiscale
        var newYMin = (Math.Abs(yMin * rankIconRectSize) + Math.Abs(yMin * rankIconGapSize) + rankIconMargin) * Prefs.UIScale;
        var newYMax = (Math.Abs(yMax * rankIconRectSize) + Math.Abs(yMax * rankIconGapSize) + rankIconMargin) * Prefs.UIScale;
        var newXMax = (Math.Abs(xMax * rankPlacementMult) + rankIconMargin * 2) * Prefs.UIScale;

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
                    x = xStart + currentlySelectedRankCategory.rankDict[rank.rankDef].displayPosition.x * rankPlacementMult,
                    y = yStart + currentlySelectedRankCategory.rankDict[rank.rankDef].displayPosition.y * rankPlacementMult,
                };

                if (currentlySelectedRankCategory.rankDict[rank.rankDef].displayPosition.x < 0)
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
            if (currentlySelectedRankCategory.rankDict[rank.rankDef].rankRequirements == null && currentlySelectedRankCategory.rankDict[rank.rankDef].rankRequirementsOneAmong == null)
            {
                continue;
            }

            var rankData = new List<RankData>();
            rankData.AddRange(currentlySelectedRankCategory.rankDict[rank.rankDef].rankRequirements ?? new List<RankData>());
            rankData.AddRange(currentlySelectedRankCategory.rankDict[rank.rankDef].rankRequirementsOneAmong ?? new List<RankData>());
            
            foreach (var rankReq in rankData)
            {
                var startPos = new Vector2(rankPos[rank.rankDef].x + rankIconRectSize/2, rankPos[rank.rankDef].y + rankIconRectSize/2);
                var endPos = new Vector2(rankPos[rankReq.rankDef].x + rankIconRectSize/2, rankPos[rankReq.rankDef].y + rankIconRectSize/2);
                    
                var rankUnlocked = compRankInfo.HasRank(rankReq.rankDef) ? Color.white : Color.grey;

                if (currentlySelectedRank != null)
                {
                    if (currentlySelectedRank.rankDef == rankReq.rankDef)
                    {
                        rankUnlocked = new Color(0.0f, 0.5f, 1f, 0.9f);
                    }
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
                x = xStart + currentlySelectedRankCategory.rankDict[rank.rankDef].displayPosition.x * rankPlacementMult,
                y = yStart + currentlySelectedRankCategory.rankDict[rank.rankDef].displayPosition.y * rankPlacementMult,
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
        
    private Vector2 scrollPosRankInfo;
    private float scrollViewHeightRankInfo = 0f;
    private void FillRankInfo(Rect rect)
    {
        var rectRankInfo = new Rect(rect);
        rectRankInfo.ContractedBy(20f);
        
        if (currentlySelectedRank != null)
        {
            const float listingHeightIncreaseMedium = 30f;
            const float listingHeightIncreaseSmall = 24f;
            const float listingHeightIncreaseGap = 12f;
            
            var viewRect = new Rect(rectRankInfo.x, rectRankInfo.y, rectRankInfo.width - 16f, scrollViewHeightRankInfo);
            scrollViewHeightRankInfo = 0f;
            
            var listingRankInfo = new Listing_Standard();
            //Start
            Widgets.BeginScrollView(rectRankInfo, ref scrollPosRankInfo, viewRect);
            listingRankInfo.Begin(viewRect);
                
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
                
            //Name
            listingRankInfo.Gap(5);
            scrollViewHeightRankInfo += 5f;
            var rankLabel = currentlySelectedRank.rankDef.label.CapitalizeFirst();
            listingRankInfo.Label(rankLabel);
            scrollViewHeightRankInfo += listingHeightIncreaseMedium;
                
            //Show day as rank
            var rankDayAmount = compRankInfo.GetDaysAsRank(currentlySelectedRank.rankDef);
            if (rankDayAmount > 0f)
            {
                listingRankInfo.Gap(5);
                scrollViewHeightRankInfo += 5f;
                Text.Font = GameFont.Small;
                listingRankInfo.Label("BEWH.Framework.RankSystem.DaysSinceRankGiven".Translate(rankDayAmount));
                scrollViewHeightRankInfo += listingHeightIncreaseSmall;
                Text.Font = GameFont.Medium;
            }
            //Unlock button
            else if (currentlySelectedRank.requirementsMet && !AlreadyUnlocked(currentlySelectedRank.rankDef))
            {   
                listingRankInfo.Gap();
                scrollViewHeightRankInfo += listingHeightIncreaseGap;
                listingRankInfo.Indent(viewRect.width * 0.25f);
                if (listingRankInfo.ButtonText("BEWH.Framework.RankSystem.UnlockRank".Translate(), widthPct: 0.5f))
                {
                    void Action() => UnlockRank(currentlySelectedRank.rankDef);
                    if (ModSettings.confirmRankUnlock)
                    {
                        var window = Dialog_MessageBox.CreateConfirmation("BEWH.Framework.RankSystem.UnlockRankConfirm".Translate(pawn, currentlySelectedRank.rankDef.label), Action, destructive: true);
                        Find.WindowStack.Add(window);
                    }
                    else
                    {
                        Action();
                    }
                }
                scrollViewHeightRankInfo += 30f;
                listingRankInfo.Outdent(viewRect.width * 0.25f);
            }
                
            listingRankInfo.GapLine(1f);
            listingRankInfo.Indent(viewRect.width * 0.02f);
            
            //Description
            listingRankInfo.Gap();
            scrollViewHeightRankInfo += listingHeightIncreaseGap;
            Text.Anchor = TextAnchor.UpperLeft;
            listingRankInfo.Label("BEWH.Framework.RankSystem.RankDescription".Translate());
            scrollViewHeightRankInfo += listingHeightIncreaseMedium;
            Text.Font = GameFont.Small;
            listingRankInfo.Label(currentlySelectedRank.rankDef.description);
            scrollViewHeightRankInfo += listingHeightIncreaseSmall * (currentlySelectedRank.rankDef.description.Split('\n').Length - 1);

            //Requirements
            listingRankInfo.Gap();
            scrollViewHeightRankInfo += listingHeightIncreaseGap;
            Text.Font = GameFont.Medium;
            listingRankInfo.Label("BEWH.Framework.RankSystem.RankRequirements".Translate());
            scrollViewHeightRankInfo += listingHeightIncreaseMedium;
            Text.Font = GameFont.Small;
            listingRankInfo.Label(currentlySelectedRank.rankText);
            scrollViewHeightRankInfo += listingHeightIncreaseSmall * (currentlySelectedRank.rankText.Split('\n').Length - 1);
                
            //Given stats
            listingRankInfo.Gap();
            scrollViewHeightRankInfo += listingHeightIncreaseGap;
            Text.Font = GameFont.Medium;
            listingRankInfo.Label("BEWH.Framework.RankSystem.RankBonuses".Translate());
            scrollViewHeightRankInfo += listingHeightIncreaseMedium;
            Text.Font = GameFont.Small;
            var rankBonusText = BuildRankBonusString(currentlySelectedRank.rankDef);
            listingRankInfo.Label(rankBonusText);
            scrollViewHeightRankInfo += listingHeightIncreaseSmall * (rankBonusText.Split('\n').Length - 1);

            scrollViewHeightRankInfo *= Prefs.UIScale;
            
            if (scrollViewHeightRankInfo < rectRankInfo.height - 4)
            {
                scrollViewHeightRankInfo = rectRankInfo.height - 4;
            }
            
            //End
            listingRankInfo.Outdent(viewRect.width * 0.02f);
            listingRankInfo.End();
            Widgets.EndScrollView();
        }
        else
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            var text = "BEWH.Framework.RankSystem.NoRankSelected".Translate();
            Widgets.Label(rectRankInfo, text);
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }

    private void UpdateRankCategoryList()
    {
        availableCategories = DefDatabase<RankCategoryDef>.AllDefsListForReading;
    }

    private void GetRanksForCategory()
    {
        availableRanksForCategory = [];

        if (currentlySelectedRankCategory == null)
        {
            return;
        }
        
        foreach (var rank in currentlySelectedRankCategory.ranks)
        {
            var rankInfo = BuildRankInfoForCategory(rank.rankDef);
            availableRanksForCategory.Add(rankInfo);
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
    
    private (bool requirementMet, string requirementText) RequirementMetAndText(RankDef rankDef)
    {
        var stringBuilder = new StringBuilder();
            
        //Limit on rank amount
        var rankLimitRequirementsMet = true;
        if (rankDef.colonyLimitOfRank.x > 0 || (rankDef.colonyLimitOfRank.x == 0 && rankDef.colonyLimitOfRank.y > 0))
        {
            var (allowed, allowedAmount, currentAmount) = gameCompRankInfo.CanHaveMoreOfRankWithInfo(rankDef);
            rankLimitRequirementsMet = allowed;
                
            var requirementColour = rankLimitRequirementsMet ? requirementMetColour : requirementNotMetColour;
                
            stringBuilder.Append("\n");
            if (rankDef.colonyLimitOfRank.y == 0)
            {
                stringBuilder.AppendLine("BEWH.Framework.RankSystem.RequirementsLimitOnlyOneEver".Translate(allowedAmount, currentAmount).Colorize(requirementColour));
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
                stringBuilder.AppendLine("BEWH.Framework.RankSystem.RequirementsLimit".Translate(allowedAmount, currentAmount, text).Colorize(requirementColour));
            }
        }
            
        //Skills
        var skillRequirementsMet = true;
        if (!rankDef.requiredSkills.NullOrEmpty())
        {
            stringBuilder.Append("\n");
            stringBuilder.AppendLine("BEWH.Framework.RankSystem.RequirementsSkills".Translate());
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
        //All
        var rankAllRequirementsMet = true;
        if (!currentlySelectedRankCategory.rankDict[rankDef].rankRequirements.NullOrEmpty())
        {
            stringBuilder.Append("\n");
            stringBuilder.AppendLine("BEWH.Framework.RankSystem.RequirementsRanks".Translate());
            foreach (var rank in currentlySelectedRankCategory.rankDict[rankDef].rankRequirements)
            {
                var rankRequirementMet = compRankInfo.HasRank(rank.rankDef) &&
                                         compRankInfo.GetDaysAsRank(rank.rankDef) >= rank.daysAs / pawn.GetStatValue(Core40kDefOf.BEWH_RankLearningFactor);
                    
                if (!rankRequirementMet)
                {
                    rankAllRequirementsMet = false;
                }
                    
                var requirementColour = rankRequirementMet ? requirementMetColour : requirementNotMetColour;
                    
                if (rank.daysAs > 0)
                {
                    stringBuilder.AppendLine(("    " + "BEWH.Framework.RankSystem.HaveBeenRankForDays".Translate(rank.rankDef.label.CapitalizeFirst(), rank.daysAs)).Colorize(requirementColour));
                }
                else
                {
                    stringBuilder.AppendLine(("    " + "BEWH.Framework.RankSystem.HaveAchievedRank".Translate(rank.rankDef.label.CapitalizeFirst())).Colorize(requirementColour));
                }
                    
            }
        }
        var rankAtLeastOneRequirementsMet = currentlySelectedRankCategory.rankDict[rankDef].rankRequirementsOneAmong.NullOrEmpty();
        if (!rankAtLeastOneRequirementsMet)
        {
            stringBuilder.Append("\n");
            stringBuilder.AppendLine("BEWH.Framework.RankSystem.RequirementsRanksAtLeastOne".Translate());
            foreach (var rank in currentlySelectedRankCategory.rankDict[rankDef].rankRequirementsOneAmong)
            {
                var rankRequirementMet = compRankInfo.HasRank(rank.rankDef) &&
                                         compRankInfo.GetDaysAsRank(rank.rankDef) >= rank.daysAs / pawn.GetStatValue(Core40kDefOf.BEWH_RankLearningFactor);
                    
                if (rankRequirementMet)
                {
                    rankAtLeastOneRequirementsMet = true;
                }
                    
                var requirementColour = rankRequirementMet ? requirementMetColour : requirementNotMetColour;
                    
                if (rank.daysAs > 0)
                {
                    stringBuilder.AppendLine(("    " + "BEWH.Framework.RankSystem.HaveBeenRankForDays".Translate(rank.rankDef.label.CapitalizeFirst(), rank.daysAs)).Colorize(requirementColour));
                }
                else
                {
                    stringBuilder.AppendLine(("    " + "BEWH.Framework.RankSystem.HaveAchievedRank".Translate(rank.rankDef.label.CapitalizeFirst())).Colorize(requirementColour));
                }
            }
        }
            
        //Traits
        //All
        var traitsAllRequirementsMet = true;
        if (!rankDef.requiredTraitsAll.NullOrEmpty())
        {
            stringBuilder.Append("\n");
            stringBuilder.AppendLine("BEWH.Framework.RankSystem.RequirementsTraitAll".Translate());
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
        if (!traitsAtLeastOneRequirementsMet)
        {
            stringBuilder.Append("\n");
            stringBuilder.AppendLine("BEWH.Framework.RankSystem.RequirementsTraitAtLeastOne".Translate());
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
                stringBuilder.AppendLine("BEWH.Framework.RankSystem.RequirementsGeneAll".Translate());
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
            if (!genesAtLeastOneRequirementsMet)
            {
                stringBuilder.Append("\n");
                stringBuilder.AppendLine("BEWH.Framework.RankSystem.RequirementsGeneAtLeastOne".Translate());
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
            stringBuilder.AppendLine("BEWH.Framework.RankSystem.IncompatibleRank".Translate());
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
            requirementText = "    " + "BEWH.Framework.CommonKeyword.None".Translate();
        }

        var requirementsMet = skillRequirementsMet 
                              && rankAllRequirementsMet 
                              && rankAtLeastOneRequirementsMet
                              && rankLimitRequirementsMet 
                              && noIncompatibleRanks 
                              && traitsAllRequirementsMet 
                              && traitsAtLeastOneRequirementsMet 
                              && genesAllRequirementsMet 
                              && genesAtLeastOneRequirementsMet;
            
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
            statBonuses = "BEWH.Framework.RankSystem.Stats".Translate() + "\n" + statBonuses;
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
        foreach (var hediff in rankDef.givesHediffs)
        {
            abilityStringBuilder.AppendLine("    " + hediff.label.CapitalizeFirst());
        }
            
        var abilityBonuses = abilityStringBuilder.ToString();
            
        if (!abilityBonuses.NullOrEmpty())
        {
            abilityBonuses = "BEWH.Framework.RankSystem.Abilities".Translate() + "\n" + abilityBonuses;
        }
        
        var customEffectStringBuilder = new StringBuilder();

        foreach (var customEffect in rankDef.customEffectDescriptions)
        {
            customEffectStringBuilder.Append("    " + customEffect.CapitalizeFirst());
        }

        var customEffects = customEffectStringBuilder.ToString();
        if (!customEffects.NullOrEmpty())
        {
            customEffects = "BEWH.Framework.RankSystem.OtherEffects".Translate() + "\n" + customEffects;
        }

        var result = "";
        if (!statBonuses.NullOrEmpty())
        {
            result = statBonuses;
        }

        if (!abilityBonuses.NullOrEmpty())
        {
            if (result.NullOrEmpty())
            {
                result = abilityBonuses;
            }
            else
            {
                result += "\n" + abilityBonuses;
            }
        }
        
        if (!customEffects.NullOrEmpty())
        {
            if (result.NullOrEmpty())
            {
                result = customEffects;
            }
            else
            {
                result += "\n" + customEffects;
            }
        }
            
        if (statBonuses.NullOrEmpty() && abilityBonuses.NullOrEmpty())
        {
            result = "    " + "BEWH.Framework.CommonKeyword.None".Translate();
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
        redoRankInfo = true;
    }
}

internal class RankInfoForTab
{
    public RankDef rankDef;
    public bool requirementsMet;
    public string rankText;
}