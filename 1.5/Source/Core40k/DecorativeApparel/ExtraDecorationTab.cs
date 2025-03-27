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
        
        public static readonly Texture2D FlippedIconTex = ContentFinder<Texture2D>.Get("UI/Decoration/flipIcon");
        
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

            float curX;
            var curY = viewRect.y;
            
            var iconSize = new Vector2(viewRect.width/RowAmount, viewRect.width/RowAmount);
            var smallIconSize = new Vector2(iconSize.x / 4, iconSize.y / 4);
            var colourButtonExtraSize = 0f;
            
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
                
                var resetChapterIconRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 30f);
                resetChapterIconRect.width /= 5;
                resetChapterIconRect.x = nameRect.xMin - resetChapterIconRect.width - nameRect.width/20;
                
                var position = new Vector2(viewRect.x, resetChapterIconRect.yMax);
                
                curX = position.x;
                curY = position.y;
                
                var currentDecorations = bodyApparel.ExtraDecorationDefs;
                var currentDecorationColours = bodyApparel.ExtraDecorationColours;
                
                for (var i = 0; i < extraDecorationDefsBody.Count; i++)
                {
                    if (!extraDecorationDefsBody[i].appliesTo.Contains(bodyApparel.def.defName))
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
                    else if (i == extraDecorationDefsBody.Count - 1)
                    {
                        curY += iconRect.height;
                    }
                    
                    iconRect = iconRect.ContractedBy(5f);
                    var hasDeco = currentDecorations.ContainsKey(extraDecorationDefsBody[i]);
                    
                    if (hasDeco)
                    {
                        Widgets.DrawStrongHighlight(iconRect.ExpandedBy(3f));
                    }
                    
                    var color = Mouse.IsOver(iconRect) ? GenUI.MouseoverColor : Color.white;
                    GUI.color = color;
                    GUI.DrawTexture(iconRect, Command.BGTexShrunk);
                    GUI.color = Color.white;
                    GUI.DrawTexture(iconRect, extraDecorationDefsBody[i].Icon);
                    
                    if (hasDeco)
                    {
                        if (currentDecorations[extraDecorationDefsBody[i]])
                        {
                            var flippedIconRect = new Rect(new Vector2(position.x + 7f, position.y + 5f), smallIconSize);
                            GUI.DrawTexture(flippedIconRect, FlippedIconTex);
                        }
                    }
                    
                    TooltipHandler.TipRegion(iconRect, extraDecorationDefsBody[i].label);
                    
                    if (Widgets.ButtonInvisible(iconRect))
                    {
                        bodyApparel.AddOrRemoveDecoration(extraDecorationDefsBody[i]);
                    }

                    var buttonRect = new Rect(new Vector2(iconRect.x, iconRect.yMax + 1f), iconRect.size);
                    buttonRect.height /= 3;
                    colourButtonExtraSize = buttonRect.height + 3f;
                    buttonRect = buttonRect.ContractedBy(2f);
                    
                    if (extraDecorationDefsBody[i].colourable && currentDecorationColours.ContainsKey(extraDecorationDefsBody[i]))
                    {
                        Widgets.DrawMenuSection(buttonRect.ExpandedBy(1f));
                        var i1 = i;
                        Widgets.DrawRectFast(buttonRect, bodyApparel.ExtraDecorationColours[extraDecorationDefsBody[i1]]);
                        if (Widgets.ButtonInvisible(buttonRect))
                        {
                            Find.WindowStack.Add( new Dialog_ColourPicker( bodyApparel.ExtraDecorationColours[extraDecorationDefsBody[i1]], ( newColour ) =>
                            {
                                bodyApparel.UpdateDecorationColour(extraDecorationDefsBody[i1], newColour);
                            } ) );
                        }
                    }
                    else
                    {
                        Widgets.DrawMenuSection(buttonRect);
                        Widgets.DrawRectFast(buttonRect, Color.gray);
                        //Make rect have cross texture like cheese showed
                    }
                }
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
                
                var resetChapterIconRect = new Rect(viewRect.x, curY, viewRect.width, 30f);
                resetChapterIconRect.width /= 5;
                resetChapterIconRect.x = nameRect.xMin - resetChapterIconRect.width - nameRect.width/20;
                
                var position = new Vector2(viewRect.x, resetChapterIconRect.yMax);
                
                curX = position.x;
                curY = position.y;
                
                var currentDecorations = helmetApparel.ExtraDecorationDefs;
                var currentDecorationColours = helmetApparel.ExtraDecorationColours;
                
                for (var i = 0; i < extraDecorationDefsHelmet.Count; i++)
                {
                    if (!extraDecorationDefsBody[i].appliesTo.Contains(helmetApparel.def.defName))
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
                    else if (i == extraDecorationDefsHelmet.Count - 1)
                    {
                        curY += iconRect.height;
                    }
                    
                    iconRect = iconRect.ContractedBy(5f);
                    var hasDeco = currentDecorations.ContainsKey(extraDecorationDefsBody[i]);
                    
                    if (hasDeco)
                    {
                        Widgets.DrawStrongHighlight(iconRect.ExpandedBy(3f));
                    }
                    
                    var color = Mouse.IsOver(iconRect) ? GenUI.MouseoverColor : Color.white;
                    GUI.color = color;
                    GUI.DrawTexture(iconRect, Command.BGTexShrunk);
                    GUI.color = Color.white;
                    GUI.DrawTexture(iconRect, extraDecorationDefsHelmet[i].Icon);
                    
                    if (hasDeco)
                    {
                        if (currentDecorations[extraDecorationDefsBody[i]])
                        {
                            var flippedIconRect = new Rect(new Vector2(position.x + 7f, position.y + 5f), smallIconSize);
                            GUI.DrawTexture(flippedIconRect, FlippedIconTex);
                        }
                    }
                    
                    TooltipHandler.TipRegion(iconRect, extraDecorationDefsHelmet[i].label);
                    
                    if (Widgets.ButtonInvisible(iconRect))
                    {
                        helmetApparel.AddOrRemoveDecoration(extraDecorationDefsHelmet[i]);
                    }
                    
                    var buttonRect = new Rect(new Vector2(iconRect.x, iconRect.yMax + 1f), iconRect.size);
                    buttonRect.height /= 3;
                    colourButtonExtraSize = buttonRect.height + 3f;
                    buttonRect = buttonRect.ContractedBy(2f);
                    
                    if (extraDecorationDefsBody[i].colourable && currentDecorationColours.ContainsKey(extraDecorationDefsBody[i]))
                    {
                        Widgets.DrawMenuSection(buttonRect);
                        var i1 = i;
                        Widgets.DrawRectFast(buttonRect, helmetApparel.ExtraDecorationColours[extraDecorationDefsBody[i1]]);
                        if (Widgets.ButtonInvisible(buttonRect))
                        {
                            Find.WindowStack.Add( new Dialog_ColourPicker( helmetApparel.ExtraDecorationColours[extraDecorationDefsBody[i1]], ( newColour ) =>
                            {
                                helmetApparel.UpdateDecorationColour(extraDecorationDefsBody[i1], newColour);
                            } ) );
                        }
                    }
                    else
                    {
                        Widgets.DrawMenuSection(buttonRect.ExpandedBy(1f));
                        Widgets.DrawRectFast(buttonRect, Color.gray);
                        //Make rect have cross texture like cheese showed
                    }
                }
            }
            
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        /*public override void OnClose(bool closeOnCancel, bool closeOnClickedOutside)
        {
            
        }

        public override void OnAccept()
        {
            
        }*/
    }
}   