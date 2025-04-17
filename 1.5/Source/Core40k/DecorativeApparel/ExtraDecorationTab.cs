using System.Collections.Generic;
using System.Linq;
using ColourPicker;
using Core40k;
using UnityEngine;
using Verse;

namespace Genes40k
{
    public class ExtraDecorationTab : ApparelColourTwoTabDrawer
    {
        private const int RowAmount = 6;

        private bool setupDone = false;

        private static float listScrollViewHeight = 0f;

        private float curY;
        
        private List<ExtraDecorationDef> extraDecorationDefsBody = new List<ExtraDecorationDef>();
        private List<ExtraDecorationDef> extraDecorationDefsHelmet = new List<ExtraDecorationDef>();
        
        private void Setup(Pawn pawn)
        {
            var allExtraDecorations = DefDatabase<ExtraDecorationDef>.AllDefs.ToList();
            foreach (var extraDecoration in allExtraDecorations)
            {
                if (!extraDecoration.HasRequirements(pawn))
                {
                    continue;
                }
                if (extraDecoration.isHelmetDecoration)
                {
                    extraDecorationDefsHelmet.Add(extraDecoration);
                }
                else
                {
                    extraDecorationDefsBody.Add(extraDecoration);
                }
            }

            extraDecorationDefsBody.SortBy(def => def.sortOrder);
            extraDecorationDefsHelmet.SortBy(def => def.sortOrder);
        }

        private void DrawRowContent(DecorativeApparelColourTwo apparel, List<ExtraDecorationDef> extraDecorationDefs, ref Vector2 position, ref Rect viewRect)
        {
            var iconSize = new Vector2(viewRect.width/RowAmount, viewRect.width/RowAmount);
            var smallIconSize = new Vector2(iconSize.x / 4, iconSize.y / 4);
            
            var currentDecorations = apparel.ExtraDecorationDefs;
            var currentDecorationColours = apparel.ExtraDecorationColours;

            var colourButtonExtraSize = 0f;

            var curX = position.x;
            
            var setY = curY;
            
            for (var i = 0; i < extraDecorationDefs.Count; i++)
            {
                if (!extraDecorationDefs[i].appliesTo.Contains(apparel.def.defName) && !extraDecorationDefs[i].appliesToAll)
                {
                    continue;
                }
                
                position = new Vector2(curX, curY);
                var iconRect = new Rect(position, iconSize);
                    
                curX += iconRect.width;

                if (i != 0 && (i+1) % RowAmount == 0)
                {
                    curY += iconRect.height + colourButtonExtraSize;
                    curX = viewRect.position.x;
                }
                else if (i == extraDecorationDefs.Count - 1)
                {
                    curY += iconRect.height + colourButtonExtraSize;
                }
                    
                iconRect = iconRect.ContractedBy(5f);
                var hasDeco = currentDecorations.ContainsKey(extraDecorationDefs[i]);
                    
                if (hasDeco)
                {
                    Widgets.DrawStrongHighlight(iconRect.ExpandedBy(3f));
                }
                    
                var color = Mouse.IsOver(iconRect) ? GenUI.MouseoverColor : Color.white;
                GUI.color = color;
                GUI.DrawTexture(iconRect, Command.BGTexShrunk);
                GUI.color = Color.white;
                GUI.DrawTexture(iconRect, extraDecorationDefs[i].Icon);
                    
                if (hasDeco)
                {
                    if (currentDecorations[extraDecorationDefs[i]])
                    {
                        var flippedIconRect = new Rect(new Vector2(position.x + 7f, position.y + 5f), smallIconSize);
                        GUI.DrawTexture(flippedIconRect, Core40kUtils.FlippedIconTex);
                    }
                }
                    
                TooltipHandler.TipRegion(iconRect, extraDecorationDefs[i].label);
                    
                if (Widgets.ButtonInvisible(iconRect))
                {
                    apparel.AddOrRemoveDecoration(extraDecorationDefs[i]);
                }

                var buttonRect = new Rect(new Vector2(iconRect.x, iconRect.yMax + 3f), iconRect.size);
                buttonRect.height /= 3;
                var butHeight = buttonRect.height;
                buttonRect = buttonRect.ContractedBy(2f);
                    
                setY = iconRect.yMax;
                
                if (extraDecorationDefs[i].colourable && currentDecorationColours.ContainsKey(extraDecorationDefs[i]))
                {
                    colourButtonExtraSize = butHeight;
                    setY += colourButtonExtraSize;
                    
                    var colourSelection = new Rect(buttonRect);
                    colourSelection.width /= 3;
                    colourSelection.width -= 3;
                    
                    Widgets.DrawMenuSection(colourSelection);
                    colourSelection = colourSelection.ContractedBy(1f);
                    
                    var i1 = i;
                    
                    Widgets.DrawRectFast(colourSelection, apparel.ExtraDecorationColours[extraDecorationDefs[i1]]);
                    if (Widgets.ButtonInvisible(colourSelection))
                    {
                        Find.WindowStack.Add( new Dialog_ColourPicker( apparel.ExtraDecorationColours[extraDecorationDefs[i1]], ( newColour ) =>
                        {
                            apparel.UpdateDecorationColour(extraDecorationDefs[i1], newColour);
                        } ) );
                    }
                    
                    var presetSelection = new Rect(buttonRect)
                    {
                        x = colourSelection.xMax + 6f,
                    };
                    presetSelection.width *= 0.66f;
                    presetSelection.width -= 3f;;
                    
                    presetSelection = presetSelection.ExpandedBy(1f);

                    if (Widgets.ButtonText(presetSelection, "Preset"))
                    {
                        SelectPreset(apparel, extraDecorationDefs[i1]);
                    }
                }
            }

            curY = setY + 34f;
        }

        private void SelectPreset(DecorativeApparelColourTwo apparel, ExtraDecorationDef extraDecoration)
        {
            var presets = extraDecoration.availablePresets;
            var list = new List<FloatMenuOption>();
            foreach (var preset in presets)
            {
                var menuOption = new FloatMenuOption(preset.label, delegate
                {
                    apparel.UpdateDecorationColour(extraDecoration, preset.colour);
                }, Core40kUtils.ColourPreview(preset.colour), Color.white);
                list.Add(menuOption);
            }
                
            if (!list.NullOrEmpty())
            {
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }
        
        public override void DrawTab(Rect rect, Pawn pawn, ref Vector2 apparelColorScrollPosition)
        {
            if (!setupDone)
            {
                setupDone = true;
                Setup(pawn);
            }
            
            GUI.BeginGroup(rect);
            var outRect = new Rect(0f, 0f, rect.width, rect.height);
            var viewRect = new Rect(0f, 0f, rect.width - 16f, listScrollViewHeight);
            Widgets.BeginScrollView(outRect, ref apparelColorScrollPosition, viewRect);
            
            var bodyApparel = (BodyDecorativeApparelColourTwo)pawn.apparel.WornApparel.FirstOrFallback(a => a is BodyDecorativeApparelColourTwo);
            
            curY = viewRect.y;
            
            if (bodyApparel != null)
            {
                //Extra decoration title
                var nameRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 30f);
                nameRect.width /= 2;
                nameRect.x += nameRect.width / 2;
                Widgets.DrawMenuSection(nameRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(nameRect, "BEWH.Framework.ExtraDecoration.BodyDecoration".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                
                var resetAllDecorations = new Rect(viewRect.x, viewRect.y, viewRect.width, 30f);
                resetAllDecorations.width /= 5;
                resetAllDecorations.x = nameRect.xMin - resetAllDecorations.width - nameRect.width/20;
                
                if (Widgets.ButtonText(resetAllDecorations, "BEWH.Framework.ExtraDecoration.RemoveAllDecorations".Translate()))
                {
                    bodyApparel.RemoveAllDecorations();
                }

                var position = new Vector2(viewRect.x, resetAllDecorations.yMax);
                
                curY = position.y;
                
                DrawRowContent(bodyApparel, extraDecorationDefsBody, ref position, ref viewRect);
            }
            
            var helmetApparel = (HeadDecorativeApparelColourTwo)pawn.apparel.WornApparel.FirstOrFallback(a => a is HeadDecorativeApparelColourTwo);

            if (helmetApparel != null)
            {
                //Extra decoration title
                var nameRect = new Rect(viewRect.x, curY, viewRect.width, 30f);
                nameRect.width /= 2;
                nameRect.x += nameRect.width / 2;
                Widgets.DrawMenuSection(nameRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(nameRect, "BEWH.Framework.ExtraDecoration.HelmetDecoration".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                
                var resetAllDecorations = new Rect(viewRect.x, curY, viewRect.width, 30f);
                resetAllDecorations.width /= 5;
                resetAllDecorations.x = nameRect.xMin - resetAllDecorations.width - nameRect.width/20;
                
                if (Widgets.ButtonText(resetAllDecorations, "BEWH.Framework.ExtraDecoration.RemoveAllDecorations".Translate()))
                {
                    helmetApparel.RemoveAllDecorations();
                }
                
                var position = new Vector2(viewRect.x, curY + resetAllDecorations.height);
                
                curY = position.y;
                
                DrawRowContent(helmetApparel, extraDecorationDefsHelmet, ref position, ref viewRect);
            }
            
            Widgets.EndScrollView();
            GUI.EndGroup();
        }
    }
}   