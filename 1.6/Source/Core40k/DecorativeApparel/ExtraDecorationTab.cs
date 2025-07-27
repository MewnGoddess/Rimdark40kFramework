using System.Collections.Generic;
using System.Linq;
using ColourPicker;
using UnityEngine;
using Verse;

namespace Core40k;

public class ExtraDecorationTab : ApparelColourTwoTabDrawer
{
    private static Core40kModSettings modSettings = null;
    public static Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();
        
    private const int RowAmount = 6;

    private bool setupDone = false;

    private static float listScrollViewHeight = 0f;

    private float curY;
        
    private List<ExtraDecorationDef> extraDecorationDefsBody = new List<ExtraDecorationDef>();
    private List<ExtraDecorationDef> extraDecorationDefsHelmet = new List<ExtraDecorationDef>();
    
    private List<ExtraDecorationPresetDef> extraDecorationPresets = new List<ExtraDecorationPresetDef>();
        
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

        foreach (var extraDecorationPresetDef in DefDatabase<ExtraDecorationPresetDef>.AllDefs)
        {
            extraDecorationPresets.Add(extraDecorationPresetDef);
        }
    }

    private void DrawRowContent(DecorativeApparelColourTwo apparel, List<ExtraDecorationDef> extraDecorationDefs, ref Vector2 position, ref Rect viewRect)
    {
        var iconSize = new Vector2(viewRect.width/RowAmount, viewRect.width/RowAmount);
        var smallIconSize = new Vector2(iconSize.x / 4, iconSize.y / 4);
            
        var currentDecorations = apparel.ExtraDecorations;

        var colourButtonExtraSize = 0f;
            
        var curX = position.x;

        var rowExpanded = false;

        extraDecorationDefs = extraDecorationDefs.Where(deco => deco.appliesToAll || deco.appliesTo.Contains(apparel.def.defName)).ToList();
            
        for (var i = 0; i < extraDecorationDefs.Count; i++)
        {
            position = new Vector2(curX, curY);
            var iconRect = new Rect(position, iconSize);
                    
            curX += iconRect.width;
                    
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
                if (currentDecorations[extraDecorationDefs[i]].Flipped)
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
                
            if (extraDecorationDefs[i].colourable && currentDecorations.ContainsKey(extraDecorationDefs[i]))
            {
                rowExpanded = true;
                var i1 = i;
                
                var bottomRect = new Rect(new Vector2(iconRect.x, iconRect.yMax + 3f), iconRect.size);
                bottomRect.height /= 3;
                bottomRect = bottomRect.ContractedBy(2f);
                
                colourButtonExtraSize = bottomRect.height;
                    
                var colourSelection = new Rect(bottomRect);
                colourSelection.width /= 3;
                colourSelection.width -= 3;
                Widgets.DrawMenuSection(colourSelection);
                colourSelection = colourSelection.ContractedBy(1f);
                Widgets.DrawRectFast(colourSelection, apparel.ExtraDecorations[extraDecorationDefs[i1]].Color);
                TooltipHandler.TipRegion(colourSelection, "BEWH.Framework.ApparelColourTwo.ChooseCustomColour".Translate());
                if (Widgets.ButtonInvisible(colourSelection))
                {
                    Find.WindowStack.Add( new Dialog_ColourPicker( apparel.ExtraDecorations[extraDecorationDefs[i1]].Color, ( newColour ) =>
                    {
                        apparel.UpdateDecorationColour(extraDecorationDefs[i1], newColour);
                    } ) );
                }
                    
                var presetSelection = new Rect(bottomRect)
                {
                    x = colourSelection.xMax + 6f,
                };
                presetSelection.width *= 0.66f;
                presetSelection.width -= 3f;;
                presetSelection = presetSelection.ExpandedBy(1f);
                TooltipHandler.TipRegion(presetSelection, "BEWH.Framework.ExtraDecoration.PresetDesc".Translate());
                if (Widgets.ButtonText(presetSelection, "BEWH.Framework.ExtraDecoration.Preset".Translate()))
                {
                    SelectPreset(apparel, extraDecorationDefs[i1]);
                }
            }
            
            if (i != 0 && (i+1) % RowAmount == 0)
            {
                curY += iconRect.height + 5f;
                if (rowExpanded)
                {
                    rowExpanded = false;
                    curY += colourButtonExtraSize + 5f;
                }
                
                curX = viewRect.position.x;
            }
            else if (i == extraDecorationDefs.Count - 1)
            {
                curY += iconRect.height + 5f;
                if (rowExpanded)
                {
                    rowExpanded = false;
                    curY += colourButtonExtraSize + 5f;
                }
            }
        }

        curY += 34f;
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

        if (extraDecoration.hasArmorColourPaletteOption)
        {
            var menuOptionMatch = new FloatMenuOption("BEWH.Framework.ExtraDecoration.UseArmourColour".Translate(), delegate
            {
                apparel.UpdateDecorationColour(extraDecoration, apparel.DrawColor);
            }, Core40kUtils.ColourPreview(apparel.DrawColor), Color.white);
            list.Add(menuOptionMatch);
        }
                
        if (list.NullOrEmpty())
        {
            var menuOptionNone = new FloatMenuOption("NoneBrackets".Translate(), null);
            list.Add(menuOptionNone);
        }
        
        Find.WindowStack.Add(new FloatMenu(list));
    }

    private ExtraDecorationPreset GetCurrentPreset(DecorativeApparelColourTwo apparel, string presetName)
    {
        var extraDecorationPresetParts = new List<ExtraDecorationPresetParts>();

        foreach (var decoration in apparel.ExtraDecorations)
        {
            var presetPart = new ExtraDecorationPresetParts()
            {
                extraDecorationDefs = decoration.Key.defName,
                flipped = decoration.Value.Flipped,
                colour = decoration.Value.Color,
            };
                
            extraDecorationPresetParts.Add(presetPart);
        }

        var extraDecorationPreset = new ExtraDecorationPreset()
        {
            extraDecorationPresetParts = extraDecorationPresetParts,
            appliesTo = apparel.def.defName,
            name = presetName
        };

        return extraDecorationPreset;
    }
        
    private void EditDecorationPreset(DecorativeApparelColourTwo apparel)
    {
        var floatMenuOptions = new List<FloatMenuOption>();
            
        var currentPreset = GetCurrentPreset(apparel, "");
            
        var presets = ModSettings.ExtraDecorationPresets.Where(deco => deco.appliesTo == apparel.def.defName);
            
        //Delete or override existing
        foreach (var preset in presets)
        {
            var menuOption = new FloatMenuOption(preset.name, delegate
            {
                currentPreset.name = preset.name;
                ModSettings.UpdatePreset(preset, currentPreset);
            }, Widgets.PlaceholderIconTex, Color.white);
            menuOption.extraPartWidth = 30f;
            menuOption.extraPartOnGUI = rect1 => Core40kUtils.DeletePreset(rect1, preset);
            menuOption.tooltip = "BEWH.Framework.ApparelColourTwo.OverridePreset".Translate(preset.name);
            floatMenuOptions.Add(menuOption);
        }
            
        //Create new
        var newPreset = new FloatMenuOption("BEWH.Framework.ApparelColourTwo.NewPreset".Translate(), delegate
        {
            Find.WindowStack.Add( new Dialog_EditExtraDecorationPresets(currentPreset));
        }, Widgets.PlaceholderIconTex, Color.white);
        floatMenuOptions.Add(newPreset);
                
        if (!floatMenuOptions.NullOrEmpty())
        {
            Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
        }
    }

    private void SelectDecorationPreset(DecorativeApparelColourTwo apparel)
    {
        var presets = ModSettings.ExtraDecorationPresets.Where(deco => deco.appliesTo == apparel.def.defName);
        var list = new List<FloatMenuOption>();
            
        foreach (var preset in presets)
        {
            var menuOption = new FloatMenuOption(preset.name, delegate
            {
                apparel.RemoveAllDecorations();
                apparel.LoadFromPreset(preset);
            });
            list.Add(menuOption);
        }

        foreach (var extraDecorationPreset in extraDecorationPresets.Where(deco => deco.appliesTo.Contains(apparel.def)))
        {
            var menuOption = new FloatMenuOption(extraDecorationPreset.label, delegate
            {
                apparel.RemoveAllDecorations();
                apparel.LoadFromPreset(extraDecorationPreset);
            });
            list.Add(menuOption);
        }
                
        if (list.NullOrEmpty())
        {
            var menuOptionNone = new FloatMenuOption("NoneBrackets".Translate(), null);
            list.Add(menuOptionNone);
        }
        
        Find.WindowStack.Add(new FloatMenu(list));
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
        var helmetApparel = (HeadDecorativeApparelColourTwo)pawn.apparel.WornApparel.FirstOrFallback(a => a is HeadDecorativeApparelColourTwo);
            
        curY = viewRect.y;
            
        //Body Decorations
        if (bodyApparel != null)
        {
            //Extra decoration title
            var nameRect = new Rect(viewRect.x, curY, viewRect.width, 30f);
            nameRect.width /= 2;
            nameRect.x += nameRect.width / 2;
            Widgets.DrawMenuSection(nameRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(nameRect, "BEWH.Framework.ExtraDecoration.BodyDecoration".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
                
            //Remove All
            var resetAllDecorations = new Rect(nameRect.xMin, curY, viewRect.width, 30f);
            resetAllDecorations.width /= 5;
            resetAllDecorations.x -= resetAllDecorations.width + nameRect.width/20;
            if (Widgets.ButtonText(resetAllDecorations, "BEWH.Framework.ExtraDecoration.RemoveAllDecorations".Translate()))
            {
                bodyApparel.RemoveAllDecorations();
            }
                
            //Preset Tab
            var presetSelectRect = new Rect(nameRect.xMax, curY, viewRect.width, 30f);
            presetSelectRect.x += nameRect.width/20;
            presetSelectRect.width /= 10;
            presetSelectRect.width -= 2;

            var presetEditRect = new Rect(presetSelectRect);
            presetEditRect.x += presetSelectRect.width + 4;
                
            //Select Preset
            TooltipHandler.TipRegion(presetSelectRect, "BEWH.Framework.ExtraDecoration.SelectDesc".Translate());
            if (Widgets.ButtonText(presetSelectRect, "BEWH.Framework.ExtraDecoration.Select".Translate()))
            {
                SelectDecorationPreset(bodyApparel);
            }
                
            //Edit Presets
            TooltipHandler.TipRegion(presetEditRect, "BEWH.Framework.ExtraDecoration.EditDesc".Translate());
            if (Widgets.ButtonText(presetEditRect, "BEWH.Framework.ExtraDecoration.Edit".Translate()))
            {
                EditDecorationPreset(bodyApparel);
            }
                
            var position = new Vector2(viewRect.x, resetAllDecorations.yMax);
            curY = position.y;    
            
            DrawRowContent(bodyApparel, extraDecorationDefsBody, ref position, ref viewRect);
            
            listScrollViewHeight = curY + 10f;
        }
            
        //Head Decorations
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
                
            //Remove All
            var resetAllDecorations = new Rect(nameRect.xMin, curY, viewRect.width, 30f);
            resetAllDecorations.width /= 5;
            resetAllDecorations.x -= resetAllDecorations.width + nameRect.width/20;
            if (Widgets.ButtonText(resetAllDecorations, "BEWH.Framework.ExtraDecoration.RemoveAllDecorations".Translate()))
            {
                helmetApparel.RemoveAllDecorations();
            }
                
            //Preset Tab
            var presetSelectRect = new Rect(nameRect.xMax, curY, viewRect.width, 30f);
            presetSelectRect.x += nameRect.width/20;
            presetSelectRect.width /= 10;
            presetSelectRect.width -= 2;

            var presetEditRect = new Rect(presetSelectRect);
            presetEditRect.x += presetSelectRect.width + 4;
                
            //Select Preset
            TooltipHandler.TipRegion(presetSelectRect, "BEWH.Framework.ExtraDecoration.SelectDesc".Translate());
            if (Widgets.ButtonText(presetSelectRect, "BEWH.Framework.ExtraDecoration.Select".Translate()))
            {
                SelectDecorationPreset(helmetApparel);
            }
                
            //Edit Presets
            TooltipHandler.TipRegion(presetEditRect, "BEWH.Framework.ExtraDecoration.EditDesc".Translate());
            if (Widgets.ButtonText(presetEditRect, "BEWH.Framework.ExtraDecoration.Edit".Translate()))
            {
                EditDecorationPreset(helmetApparel);
            }
                
            var position = new Vector2(viewRect.x, curY + resetAllDecorations.height);
            curY = position.y;   
            
            DrawRowContent(helmetApparel, extraDecorationDefsHelmet, ref position, ref viewRect);
            
            listScrollViewHeight = curY + 10f;
        }
            
        Widgets.EndScrollView();
        GUI.EndGroup();
    }
}