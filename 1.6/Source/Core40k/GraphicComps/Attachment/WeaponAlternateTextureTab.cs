using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Core40k;

public class WeaponAlternateTextureTab : CustomizerTabDrawer
{
    private const int RowAmount = 4;
    
    private static float listScrollViewHeight = 0f;
    
    private List<AlternateBaseFormDef> alternateBaseFormDefs = [];

    private ThingWithComps weapon = null;
    private CompMultiColor multiColor = null;
    
    public override void Setup(Pawn pawn)
    {
        weapon = pawn.equipment.Primary;
        multiColor = weapon.GetComp<CompMultiColor>();
        multiColor.SetOriginals();
            
        var alternateBaseFormForApparel = DefDatabase<AlternateBaseFormDef>.AllDefs
            .Where(def => def.appliesTo.Contains(weapon.def.defName))
            .ToList();
        
        alternateBaseFormDefs = [null];
        alternateBaseFormDefs.AddRange(alternateBaseFormForApparel);
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

        var nameRect = new Rect(viewRect.x, curY, viewRect.width, 30f);
        nameRect.width /= 2;
        nameRect.x += nameRect.width / 2;
        Widgets.DrawMenuSection(nameRect);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(nameRect, weapon.LabelCap);
        Text.Anchor = TextAnchor.UpperLeft;

        curY += nameRect.height;

        for (var i = 0; i < alternateBaseFormDefs.Count; i++)
        {
            var position = new Vector2(curX, curY);
            var iconRect = new Rect(position, iconSize);

            curX += iconRect.width;

            iconRect = iconRect.ContractedBy(5f);

            var alternateFormSelected = multiColor.CurrentAlternateBaseForm == alternateBaseFormDefs[i];

            if (alternateFormSelected)
            {
                Widgets.DrawStrongHighlight(iconRect.ExpandedBy(3f));
            }

            var tipTooltip = alternateBaseFormDefs[i]?.LabelCap ??
                             "BEWH.Framework.Customization.BaseTexture".Translate() +
                             multiColor.parent.def.LabelCap;
            
            GUI.color = Mouse.IsOver(iconRect) ? GenUI.MouseoverColor : Color.white;
            GUI.DrawTexture(iconRect, Command.BGTexShrunk);
            GUI.color = Color.white;
            GUI.DrawTexture(iconRect, alternateBaseFormDefs[i]?.Icon ?? multiColor.parent.def.uiIcon);
            TooltipHandler.TipRegion(iconRect, tipTooltip);

            if (Widgets.ButtonInvisible(iconRect))
            {
                multiColor.SetAlternateBaseForm(alternateBaseFormDefs[i], false);
            }

            if ((i != 0 && (i + 1) % 4 == 0) || i == alternateBaseFormDefs.Count - 1)
            {
                curY += iconSize.x;
                curX = viewRect.x;
            }
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
        multiColor.SetOriginals();
        multiColor.Notify_GraphicChanged();
    }
    
    public override void OnReset(Pawn pawn)
    {
        multiColor.Reset();
    }
}