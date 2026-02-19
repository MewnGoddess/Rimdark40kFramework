using System;
using System.Collections.Generic;
using System.Linq;
using ColourPicker;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class ArmorColoringTab : CustomizerTabDrawer
{
    private static Core40kModSettings modSettings = null;
    public static Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();
    
    private bool DevMode => Prefs.DevMode;
    
    private bool recache = true;
    private Dictionary<ThingDef, int> apparelColorMaskPageNumber = new Dictionary<ThingDef, int>();
    
    private Dictionary<ThingDef, List<MaskDef>> masks = new ();

    private List<ColourPresetDef> presets;
    
    private Dictionary<(ThingDef, MaskDef), Material> cachedMaterials = new ();
    
    private float viewRectHeight;
    
    public override void Setup(Pawn pawn)
    {
        presets = DefDatabase<ColourPresetDef>.AllDefs.Where(preset => preset.appliesToKind is PresetType.Armor or PresetType.All).ToList();
        
        var masksTemp = DefDatabase<MaskDef>.AllDefs.Where(def => def.appliesToKind is AppliesToKind.Thing or AppliesToKind.All).ToList();
        
        foreach (var item in pawn.apparel.WornApparel.Where(a => a.HasComp<CompMultiColor>()))
        {
            var multiColor = item.GetComp<CompMultiColor>();
            multiColor.SetOriginals();
            var masksForItem = masksTemp.Where(mask => mask.appliesTo.Contains(item.def.defName) || mask.appliesToKind == AppliesToKind.All).ToList();

            if (masksForItem.Any())
            {
                masksForItem.SortBy(def => def.sortOrder);
                masks.Add(item.def, masksForItem);
            }
            
            apparelColorMaskPageNumber.Add(item.def, 0);
        }
    }

    public override void DrawTab(Rect rect, Pawn pawn, ref Vector2 apparelColorScrollPosition)
    {
        var viewRect = new Rect(rect.x, rect.y, rect.width - 16f, viewRectHeight);
        Widgets.BeginScrollView(rect, ref apparelColorScrollPosition, viewRect);
        var curY = rect.y;
        var apparelMultiColor = pawn.apparel.WornApparel.Where(a => a.HasComp<CompMultiColor>());
        foreach (var item in apparelMultiColor)
        {
            var multiColor = item.GetComp<CompMultiColor>();
            //Item name
            var nameRect = new Rect(rect.x, curY, viewRect.width, 30f);
            nameRect.width /= 2;
            nameRect.x += nameRect.width / 2;
            Widgets.DrawMenuSection(nameRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(nameRect, item.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
                
            //Select button
            var selectPresetRect = new Rect(rect.x, curY, viewRect.width - 16f, 30f);
            selectPresetRect.width /= 5;
            selectPresetRect.x = nameRect.xMax + nameRect.width/20;
            if (Widgets.ButtonText(selectPresetRect, "BEWH.Framework.Customization.ColorPreset".Translate()))
            {
                var list = new List<FloatMenuOption>();
                foreach (var preset in presets.Where(p => p.appliesTo.Contains(item.def.defName) || p.appliesTo.Empty()))
                {
                    var menuOption = new FloatMenuOption(preset.label, delegate
                    {
                        multiColor.DrawColor = preset.primaryColour;
                        multiColor.DrawColorTwo = preset.secondaryColour;
                        multiColor.DrawColorThree = preset.tertiaryColour ?? preset.secondaryColour;
                        recache = true;
                            
                    }, Core40kUtils.ThreeColourPreview(preset.primaryColour, preset.secondaryColour, preset.tertiaryColour, preset.colorAmount), Color.white);
                    list.Add(menuOption);
                }
                    
                foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Armor or PresetType.All))
                {
                    var menuOption = new FloatMenuOption(preset.name.CapitalizeFirst(), delegate
                    {
                        multiColor.DrawColor = preset.primaryColour;
                        multiColor.DrawColorTwo = preset.secondaryColour;
                        multiColor.DrawColorThree = preset.tertiaryColour ?? preset.secondaryColour;
                        recache = true;
                            
                    }, Core40kUtils.ThreeColourPreview(preset.primaryColour, preset.secondaryColour, preset.tertiaryColour, 3), Color.white);
                    list.Add(menuOption);
                }
                
                if (!list.NullOrEmpty())
                {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
                
            //Save button
            var savePresetRect = new Rect(rect.x, curY, viewRect.width - 16f, 30f);
            savePresetRect.width /= 5;
            savePresetRect.x = nameRect.xMin - savePresetRect.width - nameRect.width/20;
            if (Widgets.ButtonText(savePresetRect, "BEWH.Framework.Customization.EditPreset".Translate()))
            {
                var list = new List<FloatMenuOption>();
                    
                //Delete or override existing
                foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Armor or PresetType.All))
                {
                    var menuOption = new FloatMenuOption(preset.name, delegate
                    {
                        Find.WindowStack.Add(new Dialog_ConfirmColorPresetOverride(preset, multiColor.DrawColor, multiColor.DrawColorTwo, multiColor.DrawColorThree));
                    }, Widgets.PlaceholderIconTex, Color.white);
                    menuOption.extraPartWidth = 30f;
                    menuOption.extraPartOnGUI = rect1 => Core40kUtils.DeletePreset(rect1, preset);
                    menuOption.tooltip = "BEWH.Framework.Customization.OverridePreset".Translate(preset.name);
                    list.Add(menuOption);
                }
                    
                //Create new
                var newPreset = new FloatMenuOption("BEWH.Framework.Customization.NewPreset".Translate(), delegate
                {
                    var newColourPreset = new ColourPreset
                    {
                        primaryColour = multiColor.DrawColor,
                        secondaryColour = multiColor.DrawColorTwo,
                        tertiaryColour = multiColor.DrawColorThree,
                        appliesToKind = PresetType.Armor,
                    };
                    Find.WindowStack.Add( new Dialog_EditColourPresets(newColourPreset));
                }, Widgets.PlaceholderIconTex, Color.white);
                list.Add(newPreset);
                
                if (!list.NullOrEmpty())
                {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
            
            curY += nameRect.height + 3f;
            var itemRect = new Rect(rect.x, curY, viewRect.width - 16f, 76f);
            curY += itemRect.height;
                
            if (!pawn.apparel.IsLocked(item) || DevMode)
            {
                var colorOneRect = new Rect(itemRect)
                {
                    x = itemRect.xMin,
                    y = itemRect.yMin + 2f
                };
                Rect colorTwoRect;

                var colAmount = multiColor.Props.colorMaskAmount;
                if (multiColor.CurrentAlternateBaseForm != null)
                {
                    colAmount = multiColor.CurrentAlternateBaseForm.colorAmount;
                }
                if (multiColor.MaskDef is { setsNull: false })
                {
                    colAmount = multiColor.MaskDef.colorAmount;
                }
                
                switch (colAmount)
                {
                    case 1:
                        colorOneRect = colorOneRect.ContractedBy(3);
                        PrimaryColorBox(colorOneRect, multiColor);
                        break;
                    case 2:
                        colorOneRect.width /= 2;
                        colorTwoRect = new Rect(colorOneRect)
                        {
                            x = colorOneRect.xMax
                        };
                        
                        colorOneRect = colorOneRect.ContractedBy(3);
                        PrimaryColorBox(colorOneRect, multiColor);
                        
                        colorTwoRect = colorTwoRect.ContractedBy(3);
                        SecondaryColorBox(colorTwoRect, multiColor);
                        break;
                    case 3:
                        colorOneRect.width /= 3;
                        colorTwoRect = new Rect(colorOneRect)
                        {
                            x = colorOneRect.xMax
                        };
                        var colorThreeRect = new Rect(colorTwoRect)
                        {
                            x = colorTwoRect.xMax
                        };
                        
                        colorOneRect = colorOneRect.ContractedBy(3);
                        PrimaryColorBox(colorOneRect, multiColor);
                        
                        colorTwoRect = colorTwoRect.ContractedBy(3);
                        SecondaryColorBox(colorTwoRect, multiColor);
                        
                        colorThreeRect = colorThreeRect.ContractedBy(3);
                        TertiaryColorBox(colorThreeRect, multiColor);
                        break;
                    default:
                        Log.Warning("Wrong setup in " + item + "colorAmount is more than 3 or less than 1");
                        break;
                }
                
                //Mask Stuff
                if (masks.ContainsKey(item.def) && masks[item.def].Count > 1)
                {
                    var maskRect = new Rect(itemRect)
                    {
                        y = colorOneRect.yMax + 3f,
                    };
                    maskRect.height = maskRect.width/4;
                    var arrowsEnabled = masks[item.def].Count > 4;
                    if (arrowsEnabled)
                    {
                        maskRect.height += maskRect.height / 5;
                    }
                    curY += maskRect.height;
                    maskRect = maskRect.ContractedBy(3);
                    Widgets.DrawMenuSection(maskRect.ContractedBy(-1));
                    var posRect = new Rect(maskRect);
                    posRect.width /= 4;
                    posRect.height = posRect.width;
                    
                    var maskDefs = new List<MaskDef>();

                    if (multiColor.CurrentAlternateBaseForm != null)
                    {
                        maskDefs.AddRange(masks[item.def].Where(maskDef => !multiColor.CurrentAlternateBaseForm.incompatibleMaskDefs.Contains(maskDef)));
                    }
                    else
                    {
                        maskDefs = masks[item.def];
                    }
                    
                    var num = maskDefs.Count - apparelColorMaskPageNumber[item.def]*4;
                    num = num > 4 ? 4 : num;
                    
                    //Might null error at maskDefs.Count 0?
                    var curPageMasks = maskDefs.GetRange(apparelColorMaskPageNumber[item.def]*4, num);
                    
                    for (var i = 0; i < curPageMasks.Count; i++)
                    {
                        var curPosRect = new Rect(posRect);
                        curPosRect.x += curPosRect.width * i;
                        curPosRect = curPosRect.ContractedBy(15);
                        if (!cachedMaterials.ContainsKey((item.def, curPageMasks[i])) || recache)
                        {
                            if (recache)
                            {
                                cachedMaterials = new Dictionary<(ThingDef, MaskDef), Material>();
                            }
                            var bodyType = item.def.GetModExtension<DefModExtension_ForcesBodyType>()?.forcedBodyType ?? pawn.story.bodyType;
                            
                            var alternatePath = multiColor.CurrentAlternateBaseForm?.drawnTextureIconPath;
                            var usedPath = alternatePath.NullOrEmpty() ? item.WornGraphicPath : alternatePath;
                            
                            var path = item.def.apparel.LastLayer != ApparelLayerDefOf.Overhead 
                                       && item.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover 
                                       && !item.RenderAsPack() 
                                       && usedPath != BaseContent.PlaceholderImagePath 
                                       && usedPath != BaseContent.PlaceholderGearImagePath 
                                        ? usedPath + "_" + bodyType.defName : usedPath;
                            
                            var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
                            var maskPath = curPageMasks[i]?.maskPath;
                            if (curPageMasks[i] != null && curPageMasks[i].useBodyTypes)
                            {
                                maskPath += "_" + bodyType.defName;
                            }
                            var graphic = MultiColorUtils.GetGraphic<Graphic_Multi>(path, shader, item.def.graphicData.drawSize, multiColor.DrawColor, multiColor.DrawColorTwo, multiColor.DrawColorThree, null, maskPath);
                            var material = graphic.MatSouth;
                            cachedMaterials.Add((item.def, curPageMasks[i]), material);
                            recache = false;
                        }
                        if (multiColor.MaskDef == curPageMasks[i])
                        {
                            Widgets.DrawStrongHighlight(curPosRect.ExpandedBy(6f));
                        }
                        
                        Widgets.DrawMenuSection(curPosRect.ContractedBy(-1));
                        
                        var windowMax = rect.yMax + apparelColorScrollPosition.y;
                        var windowMin = rect.yMin + apparelColorScrollPosition.y;
                        if (curPosRect.yMin > windowMin && curPosRect.yMax < windowMax)
                        {
                            Graphics.DrawTexture(curPosRect, cachedMaterials[(item.def, curPageMasks[i])].mainTexture, cachedMaterials[(item.def, curPageMasks[i])]);
                        }
                        
                        TooltipHandler.TipRegion(curPosRect, curPageMasks[i].label);
                        
                        Widgets.DrawHighlightIfMouseover(curPosRect);
                        if (Widgets.ButtonInvisible(curPosRect))
                        {
                            multiColor.MaskDef = curPageMasks[i];
                        }
                    }
                    if (arrowsEnabled)
                    {
                        var arrowBack = new Rect(maskRect)
                        {
                            height = maskRect.height / 5,
                            width = maskRect.height / 5
                        };
                        arrowBack.y = maskRect.yMax - arrowBack.height;
                        
                        if (apparelColorMaskPageNumber[item.def] > 0)
                        {
                            if (Widgets.ButtonInvisible(arrowBack))
                            {
                                apparelColorMaskPageNumber[item.def]--;
                            }
                            arrowBack = arrowBack.ContractedBy(5);
                            Widgets.DrawTextureFitted(arrowBack, Core40kUtils.ScrollBackwardIcon, 1);
                        }
                        
                        if (apparelColorMaskPageNumber[item.def] < Math.Ceiling((float)masks[item.def].Count / 4)-1)
                        {
                            var arrowForward = new Rect(arrowBack)
                            {
                                x = maskRect.xMax - arrowBack.width
                            };
                            if (Widgets.ButtonInvisible(arrowForward))
                            {
                                apparelColorMaskPageNumber[item.def]++;
                            }
                            arrowForward = arrowForward.ContractedBy(5);
                            Widgets.DrawTextureFitted(arrowForward, Core40kUtils.ScrollForwardIcon, 1);
                        }
                    }
                }
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                var rect3 = itemRect;
                rect3.x += 100f;
                Widgets.Label(rect3, ((string)"ApparelLockedCannotRecolor".Translate(pawn.Named("PAWN"), item.Named("APPAREL"))).Colorize(ColorLibrary.RedReadable));
                Text.Anchor = TextAnchor.UpperLeft;
            }
            curY += 22;
        }
        if (Event.current.type == EventType.Layout)
        {
            viewRectHeight = curY - rect.y;
        }
        Widgets.EndScrollView();
    }
    
    private void PrimaryColorBox(Rect colorOneRect, CompMultiColor item)
    {
        Widgets.DrawMenuSection(colorOneRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorOneRect, item.DrawColor);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorOneRect, "BEWH.Framework.Customization.PrimaryColor".Translate());
        TooltipHandler.TipRegion(colorOneRect, "BEWH.Framework.Customization.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorOneRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( item.DrawColor, ( newColour ) =>
            {
                item.DrawColor = newColour;
                recache = true;
            } ) );
        }
    }
    
    private void SecondaryColorBox(Rect colorTwoRect, CompMultiColor item)
    {
        Widgets.DrawMenuSection(colorTwoRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorTwoRect, item.DrawColorTwo);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorTwoRect, "BEWH.Framework.Customization.SecondaryColor".Translate());
        TooltipHandler.TipRegion(colorTwoRect, "BEWH.Framework.Customization.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorTwoRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( item.DrawColorTwo, ( newColour ) =>
            {
                item.DrawColorTwo = newColour;
                recache = true;
            } ) );
        }
    }
    
    private void TertiaryColorBox(Rect colorThreeRect, CompMultiColor item)
    {
        Widgets.DrawMenuSection(colorThreeRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorThreeRect, item.DrawColorThree);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorThreeRect, "BEWH.Framework.Customization.TertiaryColor".Translate());
        TooltipHandler.TipRegion(colorThreeRect, "BEWH.Framework.Customization.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorThreeRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( item.DrawColorThree, ( newColour ) =>
            {
                item.DrawColorThree = newColour;
                recache = true;
            } ) );
        }
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