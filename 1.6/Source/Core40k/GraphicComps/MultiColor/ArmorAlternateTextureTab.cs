using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class ArmorAlternateTextureTab : CustomizerTabDrawer
{
    private const int RowAmount = 6;
    
    private static float listScrollViewHeight = 0f;
    
    private Dictionary<Apparel, List<AlternateBaseFormDef>> alternateBaseFormDefs = [];
    
    public override void Setup(Pawn pawn)
    {
        foreach (var apparel in pawn.apparel.WornApparel.Where(apparel => apparel.HasComp<CompMultiColor>()))
        {
            var multiColor = apparel.GetComp<CompMultiColor>();
            multiColor.SetOriginals();
            
            var alternateBaseFormForApparel = DefDatabase<AlternateBaseFormDef>.AllDefs
                .Where(def => def.appliesTo.Contains(apparel.def.defName))
                .ToList();

            if (alternateBaseFormForApparel.Count == 0)
            {
                continue;
            }
            
            var toAppendToApparel = new List<AlternateBaseFormDef> { null };
            toAppendToApparel.AddRange(alternateBaseFormForApparel);

            alternateBaseFormDefs.Add(apparel, toAppendToApparel);
        }
    }
    
    public override void DrawTab(Rect rect, Pawn pawn, ref Vector2 apparelColorScrollPosition)
    {
        GUI.BeginGroup(rect);
        var outRect = new Rect(0f, 0f, rect.width, rect.height);
        var viewRect = new Rect(0f, 0f, rect.width - 16f, listScrollViewHeight);
        Widgets.BeginScrollView(outRect, ref apparelColorScrollPosition, viewRect);

        var curY = viewRect.y;
        var curX = viewRect.x;
        var iconSize = new Vector2(viewRect.width/RowAmount, viewRect.width/RowAmount);
        
        foreach (var alternateBaseFormDef in alternateBaseFormDefs)
        {
            var nameRect = new Rect(viewRect.x, curY, viewRect.width, 30f);
            nameRect.width /= 2;
            nameRect.x += nameRect.width / 2;
            Widgets.DrawMenuSection(nameRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(nameRect, alternateBaseFormDef.Key.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            curY += nameRect.height;

            var multiColorComp = alternateBaseFormDef.Key.GetComp<CompMultiColor>();

            for (var i = 0; i < alternateBaseFormDef.Value.Count; i++)
            {
                var position = new Vector2(curX, curY);
                var iconRect = new Rect(position, iconSize);
            
                curX += iconRect.width;
                    
                iconRect = iconRect.ContractedBy(5f);
            
                var alternateFormSelected = multiColorComp.CurrentAlternateBaseForm == alternateBaseFormDef.Value[i];
            
                if (alternateFormSelected)
                {
                    Widgets.DrawStrongHighlight(iconRect.ExpandedBy(3f));
                }
                
                var tipTooltip = alternateBaseFormDef.Value[i]?.LabelCap ?? "BEWH.Framework.Customization.BaseTexture".Translate() + multiColorComp.parent.def.LabelCap;
                GUI.color = Mouse.IsOver(iconRect) ? GenUI.MouseoverColor : Color.white;
                GUI.DrawTexture(iconRect, Command.BGTexShrunk);
                GUI.color = Color.white;
                GUI.DrawTexture(iconRect, alternateBaseFormDef.Value[i]?.Icon ?? multiColorComp.parent.def.uiIcon);
                TooltipHandler.TipRegion(iconRect, tipTooltip);
                
                if (Widgets.ButtonInvisible(iconRect))
                {
                    multiColorComp.SetAlternateBaseForm(alternateBaseFormDef.Value[i], true);
                }
                
                if (i != 0 && (i+1) % 4 == 0 || i == alternateBaseFormDef.Value.Count - 1)
                {
                    curY += iconSize.x;
                    curX = viewRect.x;
                }
            }

            curY += 22;
        }
            
        Widgets.EndScrollView();
        GUI.EndGroup();
    }

    public override void OnClose(Pawn pawn, bool closeOnCancel, bool closeOnClickedOutside)
    {
        OnReset(pawn);
    }

    public override void OnAccept(Pawn pawn)
    {
        foreach (var item in pawn.apparel.WornApparel.Where(a => a.HasComp<CompMultiColor>()))
        {
            var multiColor = item.GetComp<CompMultiColor>();
            multiColor.SetOriginals();
            multiColor.Notify_GraphicChanged();
        }
    }
    
    public override void OnReset(Pawn pawn)
    {
        foreach (var item in pawn.apparel.WornApparel.Where(a => a.HasComp<CompMultiColor>()))
        {
            item.GetComp<CompMultiColor>().Reset();
        } 
    }
}