using System;
using RimWorld;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColourPicker;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Core40k;

[StaticConstructorOnStartup]
public class Dialog_PaintApparelMultiColor : Window
{
    private Pawn pawn;

    Vector3 PortraitOffset = new Vector3(0f, 0f, 0.15f);

    private Vector2 apparelColorScrollPosition;
    private Dictionary<ThingDef, int> apparelColorMaskPageNumber = new Dictionary<ThingDef, int>();

    private float viewRectHeight;

    private static readonly Vector2 ButSize = new Vector2(200f, 40f);

    public override Vector2 InitialSize => new Vector2(950f, 750f);

    private bool DevMode => Prefs.DevMode;
        
    private List<ColourPresetDef> presets;

    private Dictionary<ThingDef, List<MaskDef>> masks = new ();
    
    private Dictionary<(ThingDef, MaskDef), Material> cachedMaterials = new ();

    private readonly List<ApparelMultiColorTabDef> allTabs;
        
    private readonly List<TabRecord> tabs;

    private static readonly string MainTab = "BEWH.Framework.ApparelMultiColor.MainTab".Translate();

    private string curTab;
        
    private static Core40kModSettings modSettings = null;

    public static Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();

    private bool recache = true;
    public Dialog_PaintApparelMultiColor()
    {
    }

    public Dialog_PaintApparelMultiColor(Pawn pawn)
    {
        this.pawn = pawn;
            
        presets = DefDatabase<ColourPresetDef>.AllDefs.Where(preset => preset.appliesToKind is PresetType.Armor or PresetType.All).ToList();
            
        allTabs = DefDatabase<ApparelMultiColorTabDef>.AllDefsListForReading;
            
        tabs = new List<TabRecord>();
            
        var mainTabRecord = new TabRecord(MainTab, delegate
        {
            curTab = MainTab;
        }, curTab == MainTab);
            
        tabs.Add(mainTabRecord);
        
        var masksTemp = DefDatabase<MaskDef>.AllDefs.Where(def => def.appliesToKind is AppliesToKind.Thing or AppliesToKind.All).ToList();
            
        foreach (var item in pawn.apparel.WornApparel.Where(a => a is ApparelMultiColor).Cast<ApparelMultiColor>())
        {
            item.SetOriginals();
            if (!item.def.HasModExtension<DefModExtension_EnableTabDef>())
            {
                continue;
            }
                
            var tabDefs = item.def.GetModExtension<DefModExtension_EnableTabDef>().tabDefs;

            foreach (var tabDef in tabDefs)
            {
                if (!allTabs.Contains(tabDef))
                {
                    continue;
                }
                
                var tabRecord = new TabRecord(tabDef.label, delegate
                {
                    curTab = tabDef.label;
                }, curTab == tabDef.label);

                if (!Enumerable.Any(tabs, tab => tab.label == tabRecord.label))
                {
                    tabs.Add(tabRecord);
                }
            }
            
            var masksForItem = masksTemp.Where(mask => mask.appliesTo.Contains(item.def.defName) || mask.appliesToKind == AppliesToKind.All).ToList();

            if (masksForItem.Any())
            {
                masksForItem.SortBy(def => def.sortOrder);
                masks.Add(item.def, masksForItem);
            }
            
            apparelColorMaskPageNumber.Add(item.def, 0);
        }

        curTab = tabs.FirstOrDefault()?.label;
            
        Find.TickManager.Pause();
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        var rect = new Rect(inRect)
        {
            height = Text.LineHeight * 2f
        };
        Widgets.Label(rect, "StylePawn".Translate().CapitalizeFirst() + ": " + Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name));
        Text.Font = GameFont.Small;
        inRect.yMin = rect.yMax + 4f;
        var rect2 = inRect;
        rect2.width *= 0.3f;
        rect2.yMax -= ButSize.y + 4f;
        DrawPawn(rect2);
        var rect3 = inRect;
        rect3.xMin = rect2.xMax + 10f;
        rect3.yMax -= ButSize.y + 4f;
            
        Widgets.DrawMenuSection(rect3);
        TabDrawer.DrawTabs(rect3, tabs);
        rect3 = rect3.ContractedBy(18f);
            
        if (curTab == MainTab)
        {
            DrawApparelColor(rect3);
        }
        else
        {
            var tab = allTabs.FirstOrDefault(def => def.label == curTab).tabDrawerClass;
            var tabDrawer = (ApparelMultiColorTabDrawer)Activator.CreateInstance(tab);
            tabDrawer.DrawTab(rect3, pawn, ref apparelColorScrollPosition);
        }
            
        DrawBottomButtons(inRect);
    }

    private void DrawPawn(Rect rect)
    {
        Widgets.BeginGroup(rect);
        for (var i = 0; i < 4; i++)
        {
            var position = new Rect(0f, rect.height / 4f * i, rect.width, rect.height / 4f).ContractedBy(4f);
            var image = PortraitsCache.Get(pawn, new Vector2(position.width, position.height), new Rot4(3 - i), PortraitOffset, 1.1f, supersample: true, compensateForUIScale: true, true, true, null, null, stylingStation: true);
            GUI.DrawTexture(position, image);
        }
        Widgets.EndGroup();
    }

    private void DrawApparelColor(Rect rect)
    {
        var viewRect = new Rect(rect.x, rect.y, rect.width - 16f, viewRectHeight);
        Widgets.BeginScrollView(rect, ref apparelColorScrollPosition, viewRect);
        var curY = rect.y;
        var apparelMultiColor = pawn.apparel.WornApparel.Where(a => a is ApparelMultiColor).Cast<ApparelMultiColor>();
        foreach (var item in apparelMultiColor)
        {
            //Item name
            var nameRect = new Rect(rect.x, curY, viewRect.width, 30f);
            nameRect.width /= 2;
            nameRect.x += nameRect.width / 2;
            Widgets.DrawMenuSection(nameRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(nameRect, item.Label);
            Text.Anchor = TextAnchor.UpperLeft;
                
            //Select button
            var selectPresetRect = new Rect(rect.x, curY, viewRect.width - 16f, 30f);
            selectPresetRect.width /= 5;
            selectPresetRect.x = nameRect.xMax + nameRect.width/20;
            if (Widgets.ButtonText(selectPresetRect, "BEWH.Framework.ApparelMultiColor.SelectPreset".Translate()))
            {
                var list = new List<FloatMenuOption>();
                foreach (var preset in presets.Where(p => p.appliesTo.Contains(item.def.defName) || p.appliesTo.Empty()))
                {
                    var menuOption = new FloatMenuOption(preset.label, delegate
                    {
                        item.DrawColor = preset.primaryColour;
                        item.SetSecondaryColor(preset.secondaryColour);
                        item.SetTertiaryColor(preset.tertiaryColour ?? preset.secondaryColour);
                        recache = true;
                            
                    }, Core40kUtils.ThreeColourPreview(preset.primaryColour, preset.secondaryColour, preset.tertiaryColour, preset.colorAmount), Color.white);
                    list.Add(menuOption);
                }
                    
                foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Armor or PresetType.All))
                {
                    var menuOption = new FloatMenuOption(preset.name.CapitalizeFirst(), delegate
                    {
                        item.DrawColor = preset.primaryColour;
                        item.SetSecondaryColor(preset.secondaryColour);
                        item.SetTertiaryColor(preset.tertiaryColour ?? preset.secondaryColour);
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
            if (Widgets.ButtonText(savePresetRect, "BEWH.Framework.ApparelMultiColor.EditPreset".Translate()))
            {
                var list = new List<FloatMenuOption>();
                    
                //Delete or override existing
                foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Armor or PresetType.All))
                {
                    var menuOption = new FloatMenuOption(preset.name, delegate
                    {
                        Find.WindowStack.Add(new Dialog_ConfirmColorPresetOverride(preset, item.DrawColor, item.DrawColorTwo, item.DrawColorThree));
                    }, Widgets.PlaceholderIconTex, Color.white);
                    menuOption.extraPartWidth = 30f;
                    menuOption.extraPartOnGUI = rect1 => Core40kUtils.DeletePreset(rect1, preset);
                    menuOption.tooltip = "BEWH.Framework.ApparelMultiColor.OverridePreset".Translate(preset.name);
                    list.Add(menuOption);
                }
                    
                //Create new
                var newPreset = new FloatMenuOption("BEWH.Framework.ApparelMultiColor.NewPreset".Translate(), delegate
                {
                    var newColourPreset = new ColourPreset
                    {
                        primaryColour = item.DrawColor,
                        secondaryColour = item.DrawColorTwo,
                        tertiaryColour = item.DrawColorThree,
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
                var colorOneRect = new Rect(itemRect);
                colorOneRect.width /= 3;
                colorOneRect.x = itemRect.xMin;
                colorOneRect.y = itemRect.yMin + 2f;
                
                var colorTwoRect = new Rect(colorOneRect)
                {
                    x = colorOneRect.xMax
                };

                var colorThreeRect = new Rect(colorTwoRect)
                {
                    x = colorTwoRect.xMax
                };

                colorOneRect = colorOneRect.ContractedBy(3);
                colorTwoRect = colorTwoRect.ContractedBy(3);
                colorThreeRect = colorThreeRect.ContractedBy(3);

                PrimaryColorBox(colorOneRect, item);
                SecondaryColorBox(colorTwoRect, item);
                TertiaryColorBox(colorThreeRect, item);

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
                    var num = masks[item.def].Count - apparelColorMaskPageNumber[item.def]*4;
                    num = num > 4 ? 4 : num;
                    var curPageMasks = masks[item.def].GetRange(apparelColorMaskPageNumber[item.def]*4, num);
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
                            var path = ((item.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && item.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover && !item.RenderAsPack() && item.WornGraphicPath != BaseContent.PlaceholderImagePath && item.WornGraphicPath != BaseContent.PlaceholderGearImagePath) ? (item.WornGraphicPath + "_" + pawn.story.bodyType.defName) : item.WornGraphicPath);
                            var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
                            var maskPath = curPageMasks[i]?.maskPath;
                            var graphic = MultiColorUtils.GetGraphic<Graphic_Multi>(path, shader, item.def.graphicData.drawSize, item.DrawColor, item.DrawColorTwo, item.DrawColorThree, item.Graphic.data, maskPath);
                            var material = graphic.MatSouth;
                            cachedMaterials.Add((item.def, curPageMasks[i]), material);
                            recache = false;
                        }
                        
                        if (item.MaskDef == curPageMasks[i])
                        {
                            Widgets.DrawStrongHighlight(curPosRect.ExpandedBy(6f));
                        }
                        
                        Widgets.DrawMenuSection(curPosRect.ContractedBy(-1));
                        Graphics.DrawTexture(curPosRect, cachedMaterials[(item.def, curPageMasks[i])].mainTexture, cachedMaterials[(item.def, curPageMasks[i])]);
                        
                        TooltipHandler.TipRegion(curPosRect, curPageMasks[i].label);
                        
                        Widgets.DrawHighlightIfMouseover(curPosRect);
                        if (Widgets.ButtonInvisible(curPosRect))
                        {
                            item.MaskDef = curPageMasks[i];
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

    private void PrimaryColorBox(Rect colorOneRect, ApparelMultiColor item)
    {
        Widgets.DrawMenuSection(colorOneRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorOneRect, item.DrawColor);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorOneRect, "BEWH.Framework.ApparelMultiColor.PrimaryColor".Translate());
        TooltipHandler.TipRegion(colorOneRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
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
    
    private void SecondaryColorBox(Rect colorTwoRect, ApparelMultiColor item)
    {
        Widgets.DrawMenuSection(colorTwoRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorTwoRect, item.DrawColorTwo);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorTwoRect, "BEWH.Framework.ApparelMultiColor.SecondaryColor".Translate());
        TooltipHandler.TipRegion(colorTwoRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorTwoRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( item.DrawColorTwo, ( newColour ) =>
            {
                item.SetSecondaryColor(newColour);
                recache = true;
            } ) );
        }
    }
    
    private void TertiaryColorBox(Rect colorThreeRect, ApparelMultiColor item)
    {
        Widgets.DrawMenuSection(colorThreeRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorThreeRect, item.DrawColorThree);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorThreeRect, "BEWH.Framework.ApparelMultiColor.TertiaryColor".Translate());
        TooltipHandler.TipRegion(colorThreeRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorThreeRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( item.DrawColorThree, ( newColour ) =>
            {
                item.SetTertiaryColor(newColour);
                recache = true;
            } ) );
        }
    }

    private void DrawBottomButtons(Rect inRect)
    {
        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Cancel".Translate()))
        {
            Close();
        }
        if (Widgets.ButtonText(new Rect(inRect.xMin + inRect.width / 2f - ButSize.x / 2f, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Reset".Translate()))
        {
            Reset();
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
        }
        if (Widgets.ButtonText(new Rect(inRect.xMax - ButSize.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Accept".Translate()))
        {
            Accept();
            Close();
        }
    }

    public override void Close(bool doCloseSound = true)
    {
        if (closeOnCancel || closeOnClickedOutside)
        {
            Reset();
        }

        foreach (var tab in allTabs)
        {
            if (tab.label == MainTab)
            {
                continue;
            }
            var tabDrawer = (ApparelMultiColorTabDrawer)Activator.CreateInstance(tab.tabDrawerClass);
            tabDrawer.OnClose(pawn, closeOnCancel, closeOnClickedOutside);
        }
        
        base.Close(doCloseSound);
    }

    private void Reset()
    {
        foreach (var item in pawn.apparel.WornApparel.Where(a => a is ApparelMultiColor).Cast<ApparelMultiColor>())
        {
            item.Reset();
        }
        
        foreach (var tab in allTabs)
        {
            if (tab.label == MainTab)
            {
                continue;
            }
            var tabDrawer = (ApparelMultiColorTabDrawer)Activator.CreateInstance(tab.tabDrawerClass);
            tabDrawer.OnReset(pawn);
        }
        
        recache = true;
        
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void Accept()
    {
        foreach (var item in pawn.apparel.WornApparel.Where(a => a is ApparelMultiColor).Cast<ApparelMultiColor>())
        {
            item.Notify_ColorChanged();
            item.SetOriginals();
        }

        foreach (var tab in allTabs)
        {
            if (tab.label == MainTab)
            {
                continue;
            }
            var tabDrawer = (ApparelMultiColorTabDrawer)Activator.CreateInstance(tab.tabDrawerClass);
            tabDrawer.OnAccept(pawn);
        }
    }
}