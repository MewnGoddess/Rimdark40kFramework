using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Core40k;

public class AlternateTextureBaseTab : CustomizerTabDrawer
{
    protected static Core40kModSettings modSettings = null;
    public static Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();

    protected List<CompAlternateTexture> alternateComps = [];
    private Dictionary<CompAlternateTexture, List<AlternateBaseFormDef>> alternateBaseFormDefs = [];

    private static float listScrollViewHeight = 0f;
    
    public override void Setup(Pawn pawn)
    {
        SetupHook(pawn);
        foreach (var alternateComp in alternateComps)
        {
            alternateComp.SetOriginals();
            
            var alternateBaseFormForApparel = DefDatabase<AlternateBaseFormDef>.AllDefs
                .Where(def => def.appliesTo.Contains(alternateComp.parent.def.defName))
                .ToList();

            if (alternateBaseFormForApparel.Count == 0)
            {
                continue;
            }
            
            var toAppendToApparel = new List<AlternateBaseFormDef> { null };
            toAppendToApparel.AddRange(alternateBaseFormForApparel);

            alternateBaseFormDefs.Add(alternateComp, toAppendToApparel);
        }
    }

    protected virtual void SetupHook(Pawn pawn) { }

     public override void DrawTab(Rect rect, Pawn pawn, ref Vector2 scrollPosition)
    {
        GUI.BeginGroup(rect);
        var outRect = new Rect(0f, 0f, rect.width, rect.height);
        var viewRect = new Rect(0f, 0f, rect.width - 16f, listScrollViewHeight);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

        var curY = viewRect.y;
        var curX = viewRect.x;
        var iconSize = new Vector2(viewRect.width/ModSettings.decorationsPerRow, viewRect.width/ModSettings.decorationsPerRow);
        
        foreach (var alternateComp in alternateComps)
        {
            var nameRect = new Rect(viewRect.x, curY, viewRect.width, 30f);
            nameRect.width /= 2;
            nameRect.x += nameRect.width / 2;
            Widgets.DrawMenuSection(nameRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(nameRect, alternateComp.parent.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            curY += nameRect.height;

            for (var i = 0; i < alternateBaseFormDefs[alternateComp].Count; i++)
            {
                var position = new Vector2(curX, curY);
                var iconRect = new Rect(position, iconSize);
            
                curX += iconRect.width;
                    
                iconRect = iconRect.ContractedBy(5f);
            
                var alternateFormSelected = alternateComp.CurrentAlternateBaseForm == alternateBaseFormDefs[alternateComp][i];
            
                if (alternateFormSelected)
                {
                    Widgets.DrawStrongHighlight(iconRect.ExpandedBy(3f));
                }
                
                var tipTooltip = alternateBaseFormDefs[alternateComp][i]?.LabelCap ?? "BEWH.Framework.Customization.BaseTexture".Translate() + alternateComp.parent.def.LabelCap;
                GUI.color = Mouse.IsOver(iconRect) ? GenUI.MouseoverColor : Color.white;
                GUI.DrawTexture(iconRect, Command.BGTexShrunk);
                GUI.color = Color.white;
                GUI.DrawTexture(iconRect, alternateBaseFormDefs[alternateComp][i]?.Icon ?? alternateComp.parent.def.uiIcon);
                TooltipHandler.TipRegion(iconRect, tipTooltip);
                
                if (Widgets.ButtonInvisible(iconRect))
                {
                    alternateComp.SetAlternateBaseForm(alternateBaseFormDefs[alternateComp][i], true);
                }
                
                if (i != 0 && (i+1) % 4 == 0 || i == alternateBaseFormDefs[alternateComp].Count - 1)
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
        foreach (var alternateComp in alternateComps)
        {
            alternateComp.SetOriginals();
            alternateComp.Notify_GraphicChanged();
        }
    }
    
    public override void OnReset(Pawn pawn)
    {
        foreach (var alternateComp in alternateComps)
        {
            alternateComp.Reset();
        }
    }
}