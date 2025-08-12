using System.Collections.Generic;
using System.Linq;
using ColourPicker;
using UnityEngine;
using Verse;

namespace Core40k;

public class ExtraDecorationTab : ApparelMultiColorTabDrawer
{
    private static Core40kModSettings modSettings = null;
    public static Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();
        
    private const int RowAmount = 6;

    private bool setupDone = false;

    private static float listScrollViewHeight = 0f;

    private float curY;
    
    private Dictionary<ExtraDecorationDef, List<MaskDef>> masks = new ();
    
    private Dictionary<(ExtraDecorationDef, MaskDef), Material> cachedMaterials = new ();
    
    private bool recache = true;
        
    private List<ExtraDecorationDef> extraDecorationDefsBody = new List<ExtraDecorationDef>();
    private List<ExtraDecorationDef> extraDecorationDefsHelmet = new List<ExtraDecorationDef>();
    
    private List<ExtraDecorationPresetDef> extraDecorationPresets = new List<ExtraDecorationPresetDef>();
        
    private void Setup(Pawn pawn)
    {
        var allExtraDecorations = DefDatabase<ExtraDecorationDef>.AllDefs.ToList();
        
        var masksTemp = DefDatabase<MaskDef>.AllDefs.Where(def => def.appliesToKind is AppliesToKind.ExtraDecoration or AppliesToKind.All).ToList();

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
            
            var masksForItem = masksTemp.Where(mask => mask.appliesTo.Contains(extraDecoration.defName)).ToList();

            if (masksForItem.Any())
            {
                masksForItem.SortBy(def => def.sortOrder);
                masks.Add(extraDecoration, masksForItem);
            }
        }

        extraDecorationDefsBody.SortBy(def => def.sortOrder);
        extraDecorationDefsHelmet.SortBy(def => def.sortOrder);

        foreach (var extraDecorationPresetDef in DefDatabase<ExtraDecorationPresetDef>.AllDefs)
        {
            extraDecorationPresets.Add(extraDecorationPresetDef);
        }
    }

    private void DrawRowContent(DecorativeApparelMultiColor apparel, List<ExtraDecorationDef> extraDecorationDefs, ref Vector2 position, ref Rect viewRect)
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
                
                colourButtonExtraSize = bottomRect.height*2;

                Rect colourSelection;
                Rect colourSelectionTwo;
                Rect colourSelectionThree;
                Rect presetSelection;
                
                switch (apparel.ExtraDecorations[extraDecorationDefs[i]].maskDef?.colorAmount ?? extraDecorationDefs[i].colorAmount)
                {
                    case 1:
                        colourSelection = new Rect(bottomRect);
                        
                        colourSelection = colourSelection.ContractedBy(2f);
                        Widgets.DrawMenuSection(colourSelection);
                        colourSelection = colourSelection.ContractedBy(1f);
                        Widgets.DrawRectFast(colourSelection, apparel.ExtraDecorations[extraDecorationDefs[i1]].Color);
                        TooltipHandler.TipRegion(colourSelection, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                        if (Widgets.ButtonInvisible(colourSelection))
                        {
                            Find.WindowStack.Add( new Dialog_ColourPicker( apparel.ExtraDecorations[extraDecorationDefs[i1]].Color, ( newColour ) =>
                            {
                                apparel.UpdateDecorationColourOne(extraDecorationDefs[i1], newColour);
                                recache = true;
                            } ) );
                        }
                        break;
                    case 2:
                        colourSelection = new Rect(bottomRect);
                        colourSelection.width /= 2;
                        colourSelectionTwo = new Rect(colourSelection)
                        {
                            x = colourSelection.xMax
                        };

                        colourSelection = colourSelection.ContractedBy(2f);
                        Widgets.DrawMenuSection(colourSelection);
                        colourSelection = colourSelection.ContractedBy(1f);
                        Widgets.DrawRectFast(colourSelection, apparel.ExtraDecorations[extraDecorationDefs[i1]].Color);
                        TooltipHandler.TipRegion(colourSelection, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                        if (Widgets.ButtonInvisible(colourSelection))
                        {
                            Find.WindowStack.Add( new Dialog_ColourPicker( apparel.ExtraDecorations[extraDecorationDefs[i1]].Color, ( newColour ) =>
                            {
                                apparel.UpdateDecorationColourOne(extraDecorationDefs[i1], newColour);
                                recache = true;
                            } ) );
                        }
                        
                        colourSelectionTwo = colourSelectionTwo.ContractedBy(2f);
                        Widgets.DrawMenuSection(colourSelectionTwo);
                        colourSelectionTwo = colourSelectionTwo.ContractedBy(1f);
                        Widgets.DrawRectFast(colourSelectionTwo, apparel.ExtraDecorations[extraDecorationDefs[i1]].ColorTwo);
                        TooltipHandler.TipRegion(colourSelectionTwo, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                        if (Widgets.ButtonInvisible(colourSelectionTwo))
                        {
                            Find.WindowStack.Add( new Dialog_ColourPicker( apparel.ExtraDecorations[extraDecorationDefs[i1]].ColorTwo, ( newColour ) =>
                            {
                                apparel.UpdateDecorationColourTwo(extraDecorationDefs[i1], newColour);
                                recache = true;
                            } ) );
                        }
                        break;
                    case 3:
                        colourSelection = new Rect(bottomRect);
                        colourSelection.width /= 3;
                        colourSelectionTwo = new Rect(colourSelection)
                        {
                            x = colourSelection.xMax
                        };
                        colourSelectionThree = new Rect(colourSelectionTwo)
                        {
                            x = colourSelectionTwo.xMax
                        };

                        colourSelection = colourSelection.ContractedBy(2f);
                        Widgets.DrawMenuSection(colourSelection);
                        colourSelection = colourSelection.ContractedBy(1f);
                        Widgets.DrawRectFast(colourSelection, apparel.ExtraDecorations[extraDecorationDefs[i1]].Color);
                        TooltipHandler.TipRegion(colourSelection, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                        if (Widgets.ButtonInvisible(colourSelection))
                        {
                            Find.WindowStack.Add( new Dialog_ColourPicker( apparel.ExtraDecorations[extraDecorationDefs[i1]].Color, ( newColour ) =>
                            {
                                apparel.UpdateDecorationColourOne(extraDecorationDefs[i1], newColour);
                                recache = true;
                            } ) );
                        }
                        
                        colourSelectionTwo = colourSelectionTwo.ContractedBy(2f);
                        Widgets.DrawMenuSection(colourSelectionTwo);
                        colourSelectionTwo = colourSelectionTwo.ContractedBy(1f);
                        Widgets.DrawRectFast(colourSelectionTwo, apparel.ExtraDecorations[extraDecorationDefs[i1]].ColorTwo);
                        TooltipHandler.TipRegion(colourSelectionTwo, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                        if (Widgets.ButtonInvisible(colourSelectionTwo))
                        {
                            Find.WindowStack.Add( new Dialog_ColourPicker( apparel.ExtraDecorations[extraDecorationDefs[i1]].ColorTwo, ( newColour ) =>
                            {
                                apparel.UpdateDecorationColourTwo(extraDecorationDefs[i1], newColour);
                                recache = true;
                            } ) );
                        }
                        
                        colourSelectionThree = colourSelectionThree.ContractedBy(2f);
                        Widgets.DrawMenuSection(colourSelectionThree);
                        colourSelectionThree = colourSelectionThree.ContractedBy(1f);
                        Widgets.DrawRectFast(colourSelectionThree, apparel.ExtraDecorations[extraDecorationDefs[i1]].ColorThree);
                        TooltipHandler.TipRegion(colourSelectionThree, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                        if (Widgets.ButtonInvisible(colourSelectionThree))
                        {
                            Find.WindowStack.Add( new Dialog_ColourPicker( apparel.ExtraDecorations[extraDecorationDefs[i1]].ColorThree, ( newColour ) =>
                            {
                                apparel.UpdateDecorationColourThree(extraDecorationDefs[i1], newColour);
                                recache = true;
                            } ) );
                        }
                        break;
                    default:
                        Log.Warning("Wrong setup in " + extraDecorationDefs[i] + "colorAmount is more than 3 or less than 1");
                        break;
                }
                
                presetSelection = new Rect(bottomRect)
                {
                    y = bottomRect.yMax
                };
                if (masks.ContainsKey(extraDecorationDefs[i]))
                {
                    presetSelection.width /= 2;
                }
                var maskSelection = new Rect(presetSelection)
                {
                    x = presetSelection.xMax
                };
                presetSelection = presetSelection.ContractedBy(1f);
                maskSelection = maskSelection.ContractedBy(1f);
                Text.Font = GameFont.Tiny;
                TooltipHandler.TipRegion(presetSelection, "BEWH.Framework.ExtraDecoration.PresetDesc".Translate());
                if (Widgets.ButtonText(presetSelection, "BEWH.Framework.ExtraDecoration.Preset".Translate()))
                {
                    SelectPreset(apparel, extraDecorationDefs[i1]);
                }
                if (masks.ContainsKey(extraDecorationDefs[i]))
                {
                    TooltipHandler.TipRegion(maskSelection, "BEWH.Framework.ExtraDecoration.MaskDesc".Translate());
                    if (Widgets.ButtonText(maskSelection, "BEWH.Framework.ExtraDecoration.Mask".Translate()))
                    {
                        SelectMask(apparel, extraDecorationDefs[i1]);
                    }
                }
                Text.Font = GameFont.Small;
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

    private void SelectMask(DecorativeApparelMultiColor apparel, ExtraDecorationDef extraDecoration)
    {
        var list = new List<FloatMenuOption>();

        foreach (var mask in masks.TryGetValue(extraDecoration))
        {
            if (apparel.ExtraDecorations[extraDecoration].maskDef == mask)
            {
                continue;
            }
            
            if (!cachedMaterials.ContainsKey((extraDecoration, mask)) || recache)
            {
                if (recache)
                {
                    cachedMaterials = new Dictionary<(ExtraDecorationDef, MaskDef), Material>();
                }

                var path = extraDecoration.drawnTextureIconPath;
                var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
                var graphic = MultiColorUtils.GetGraphic<Graphic_Multi>(path, shader, Vector2.one, apparel.ExtraDecorations[extraDecoration].Color, apparel.ExtraDecorations[extraDecoration].Color, apparel.ExtraDecorations[extraDecoration].Color, null, mask?.maskPath);
                var material = graphic.MatSouth;
                cachedMaterials.Add((extraDecoration, mask), material);
                recache = false;
            }
            
            var menuOption = new FloatMenuOption(mask.label, delegate
            {
                apparel.UpdateDecorationMask(extraDecoration, mask);
                recache = true;
            }, (Texture2D)null, Color.white, mouseoverGuiAction: rect => Graphics.DrawTexture(rect, cachedMaterials[(extraDecoration, mask)].mainTexture, cachedMaterials[(extraDecoration, mask)]));
            list.Add(menuOption);
        }
        
        if (list.NullOrEmpty())
        {
            var menuOptionNone = new FloatMenuOption("NoneBrackets".Translate(), null);
            list.Add(menuOptionNone);
        }
        
        Find.WindowStack.Add(new FloatMenu(list));
    }

    private void SelectPreset(DecorativeApparelMultiColor apparel, ExtraDecorationDef extraDecoration)
    {
        var presets = extraDecoration.availablePresets;
        var list = new List<FloatMenuOption>();
        var colorAmount = apparel.ExtraDecorations[extraDecoration].maskDef?.colorAmount ?? extraDecoration.colorAmount;
        foreach (var preset in presets)
        {
            var menuOption = new FloatMenuOption(preset.label, delegate
            {
                apparel.UpdateDecorationColourOne(extraDecoration, preset.colour);
                apparel.UpdateDecorationColourTwo(extraDecoration, preset.colourTwo ?? Color.white);
                apparel.UpdateDecorationColourThree(extraDecoration, preset.colourThree ?? Color.white);
                recache = true;
            }, Core40kUtils.ThreeColourPreview(preset.colour, preset.colourTwo, preset.colourThree, colorAmount), Color.white);
            list.Add(menuOption);
        }

        if (extraDecoration.hasArmorColourPaletteOption)
        {
            var menuOptionMatch = new FloatMenuOption("BEWH.Framework.ExtraDecoration.UseArmourColour".Translate(), delegate
            {
                apparel.SetArmorColors(extraDecoration);
            }, Core40kUtils.ThreeColourPreview(apparel.DrawColor, apparel.DrawColorTwo, apparel.DrawColorThree, colorAmount), Color.white);
            list.Add(menuOptionMatch);
        }

        var col1 = extraDecoration.defaultColour ?? (extraDecoration.useArmorColourAsDefault ? apparel.DrawColor : Color.white);
        var col2 = extraDecoration.defaultColourTwo ?? (extraDecoration.useArmorColourAsDefault ? apparel.DrawColorTwo : Color.white);
        var col3 = extraDecoration.defaultColourThree ?? (extraDecoration.useArmorColourAsDefault ? apparel.DrawColorThree : Color.white);
        
        var menuOptionDefault = new FloatMenuOption("BEWH.Framework.ExtraDecoration.SetDefaultColor".Translate(), delegate
        {
            apparel.SetDefaultColors(extraDecoration);
        }, Core40kUtils.ThreeColourPreview(col1, col2, col3, colorAmount), Color.white);
        list.Add(menuOptionDefault);
                
        if (list.NullOrEmpty())
        {
            var menuOptionNone = new FloatMenuOption("NoneBrackets".Translate(), null);
            list.Add(menuOptionNone);
        }
        
        Find.WindowStack.Add(new FloatMenu(list));
    }

    private ExtraDecorationPreset GetCurrentPreset(DecorativeApparelMultiColor apparel, string presetName)
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
        
    private void EditDecorationPreset(DecorativeApparelMultiColor apparel)
    {
        var floatMenuOptions = new List<FloatMenuOption>();
            
        var currentPreset = GetCurrentPreset(apparel, "");
            
        var presets = ModSettings.ExtraDecorationPresets.Where(deco => deco.appliesTo == apparel.def.defName);
            
        //Delete or override existing
        foreach (var preset in presets)
        {
            var menuOption = new FloatMenuOption(preset.name, delegate
            {
                Find.WindowStack.Add(new Dialog_ConfirmDecorationPresetOverride(preset, currentPreset));
            }, Widgets.PlaceholderIconTex, Color.white);
            menuOption.extraPartWidth = 30f;
            menuOption.extraPartOnGUI = rect1 => Core40kUtils.DeletePreset(rect1, preset);
            menuOption.tooltip = "BEWH.Framework.ApparelMultiColor.OverridePreset".Translate(preset.name);
            floatMenuOptions.Add(menuOption);
        }
            
        //Create new
        var newPreset = new FloatMenuOption("BEWH.Framework.ApparelMultiColor.NewPreset".Translate(), delegate
        {
            Find.WindowStack.Add( new Dialog_EditExtraDecorationPresets(currentPreset));
        }, Widgets.PlaceholderIconTex, Color.white);
        floatMenuOptions.Add(newPreset);
                
        if (!floatMenuOptions.NullOrEmpty())
        {
            Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
        }
    }

    private void SelectDecorationPreset(DecorativeApparelMultiColor apparel)
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
            
        var bodyApparel = (BodyDecorativeApparelMultiColor)pawn.apparel.WornApparel.FirstOrFallback(a => a is BodyDecorativeApparelMultiColor);
        var helmetApparel = (HeadDecorativeApparelMultiColor)pawn.apparel.WornApparel.FirstOrFallback(a => a is HeadDecorativeApparelMultiColor);
            
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