using RimWorld;
using System.Collections.Generic;
using System.Linq;
using ColourPicker;
using UnityEngine;
using VEF.Utils;
using Verse;
using Verse.Sound;

namespace Core40k;

[StaticConstructorOnStartup]
public class Dialog_PaintWeaponMultiColor : Window
{
    private Pawn pawn;

    private static readonly Vector2 ButSize = new Vector2(200f, 40f);

    public override Vector2 InitialSize => new Vector2(950f, 750f);

    private bool DevMode => Prefs.DevMode;
        
    private List<ColourPresetDef> presets;

    private Dictionary<WeaponDecorationTypeDef, List<WeaponDecorationDef>> weaponDecorations = new();
        
    private static Core40kModSettings cachedModSettings = null;

    public static Core40kModSettings ModSettings => cachedModSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();

    private Material cachedMaterial = null;
    
    private List<AlternateBaseFormDef> alternateBaseForms = [];
    private AlternateBaseFormDef selectedAlternateBaseForm = null;

    private ThingWithComps weapon;
    private CompMultiColor MultiColorComp => weapon.GetComp<CompMultiColor>();
    private CompWeaponDecoration WeaponDecorationComp => weapon.GetComp<CompWeaponDecoration>();

    private int weaponMaskAmount => selectedAlternateBaseForm?.colorAmount ?? MultiColorComp.Props.colorMaskAmount;
    
    private bool recache = true;
    
    private (Color col1, Color col2, Color col3) originalColor = (Color.white, Color.white, Color.white);
    
    private List<WeaponDecorationPresetDef> weaponDecorationPresets = new List<WeaponDecorationPresetDef>();
    
    public Dialog_PaintWeaponMultiColor()
    {
    }

    public Dialog_PaintWeaponMultiColor(Pawn pawn, ThingWithComps weaponMultiColor)
    {
        Setup(pawn, weaponMultiColor);
    }
    
    private void Setup(Pawn pawn, ThingWithComps weaponMultiColor)
    {
        this.pawn = pawn;
            
        presets = DefDatabase<ColourPresetDef>.AllDefs.Where(preset => preset.appliesToKind is PresetType.Weapon or PresetType.All).ToList();

        weapon = weaponMultiColor;

        originalColor = (MultiColorComp?.Props?.defaultPrimaryColor ?? (weapon.def.MadeFromStuff ? weapon.def.GetColorForStuff(weapon.Stuff) : Color.white), MultiColorComp?.Props?.defaultSecondaryColor ?? Color.white, MultiColorComp?.Props?.defaultTertiaryColor ?? Color.white);

        if (weapon.HasComp<CompWeaponDecoration>())
        {
            var weaponDecoGrouping = DefDatabase<WeaponDecorationDef>.AllDefs.Where(def => def.appliesTo.Contains(weapon.def.defName) || def.appliesToAll).GroupBy(def => def.decorationType);
            foreach (var grouping in weaponDecoGrouping)
            {
                weaponDecorations.Add(grouping.Key, grouping.ToList());
            }
            
            WeaponDecorationComp.SetOriginals();
            WeaponDecorationComp.RemoveInvalidDecorations(pawn);
        }
        
        foreach (var weaponDecorationPresetDef in DefDatabase<WeaponDecorationPresetDef>.AllDefs)
        {
            weaponDecorationPresets.Add(weaponDecorationPresetDef);
        }
        
        alternateBaseForms = DefDatabase<AlternateBaseFormDef>.AllDefs.Where(def => def.appliesTo.Contains(weaponMultiColor.def.defName)).ToList();

        if (MultiColorComp != null)
        {
            selectedAlternateBaseForm = MultiColorComp.currentAlternateBaseForm;
        }
        
        Find.TickManager.Pause();
    }
    
    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleCenter;
        var rect = new Rect(inRect)
        {
            height = Text.LineHeight * 2f
        }; 
        Widgets.Label(rect, weapon.Label.CapitalizeFirst());
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;
        
        inRect.yMin = rect.yMax + 4f;
        
        var rect2 = inRect;
        rect2.yMax -= ButSize.y + 4f;

        Rect iconRect;

        if (!weaponDecorations.NullOrEmpty())
        {
            rect2.width /= 2;

            var rect3 = new Rect(rect2)
            {
                x = rect2.xMax
            };

            iconRect = DrawGraphicIcon(rect2);
            if (MultiColorComp != null)
            {
                DrawWeaponColoring(rect2, iconRect);
            }
            DrawDecorations(rect3);
        }
        else
        {
            iconRect = DrawGraphicIcon(rect2);
            if (MultiColorComp != null)
            {
                DrawWeaponColoring(rect2, iconRect);
            }
        }
        
        DrawBottomButtons(inRect);
    }

    private Rect DrawGraphicIcon(Rect inRect)
    {
        var iconRect = inRect.TakeTopPart(inRect.height / 2f);
        iconRect.width = iconRect.height;
        iconRect.x += inRect.width / 2 - iconRect.width / 2;

        iconRect = iconRect.ContractedBy(1);
        
        if (cachedMaterial == null || recache)
        {
            recache = false;
            
            var path = selectedAlternateBaseForm?.drawnTextureIconPath ?? weapon.def.graphicData.texPath;
            var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;

            Graphic graphic;
            if (MultiColorComp != null)
            {
                graphic = MultiColorUtils.GetGraphic<Graphic_Single>(path, shader, weapon.def.graphicData.drawSize, MultiColorComp.DrawColor, MultiColorComp.DrawColorTwo, MultiColorComp.DrawColorThree, weapon.Graphic.data);
            }
            else
            {
                graphic = GraphicDatabase.Get<Graphic_Single>(path, shader, weapon.def.graphicData.drawSize, weapon.DrawColor, weapon.DrawColorTwo, weapon.def.graphicData);
            }
            
            var material = graphic.MatSouth;
            cachedMaterial = material;
        }
        
        Widgets.DrawMenuSection(iconRect.ContractedBy(-1));

        Widgets.DrawTextureFitted(iconRect, cachedMaterial.mainTexture, 1f, cachedMaterial);
        
        if (WeaponDecorationComp != null)
        {
            foreach (var graphic in WeaponDecorationComp.Graphics)
            {
                var offset = Vector3.zero;
                var drawSize = graphic.Key.drawSize;
                if (graphic.Key.weaponSpecificDrawData != null && graphic.Key.weaponSpecificDrawData.TryGetValue(weapon.def.defName, out var value))
                {
                    offset = value.OffsetForRot(Rot4.Invalid);
                    drawSize *= value.scale;
                }
                else if(graphic.Key.drawData != null)
                {
                    offset = graphic.Key.drawData.OffsetForRot(Rot4.Invalid);
                    drawSize *= graphic.Key.drawData.scale;
                }
                
                var offsetRect = new Rect(iconRect);
                offsetRect.width *= drawSize.x;
                offsetRect.height *= drawSize.y;
                offsetRect.width /= weapon.def.graphicData.drawSize.x;
                offsetRect.height /= weapon.def.graphicData.drawSize.y;
                offsetRect.x = iconRect.center.x - offsetRect.width / 2;
                offsetRect.y = iconRect.center.y - offsetRect.height / 2;

                var sizeDiff = iconRect.size - offsetRect.size;
                
                offsetRect.x += (offset.x * sizeDiff.x + offsetRect.width * offset.x) / weapon.def.graphicData.drawSize.x;
                offsetRect.y -= (offset.z * sizeDiff.y + offsetRect.height * offset.z) / weapon.def.graphicData.drawSize.y;

                //offsetRect.x += extraOffst.x;
                //offsetRect.y += extraOffst.y;
                
                Widgets.DrawTextureFitted(offsetRect, graphic.Value.MatSouth.mainTexture, 1f, graphic.Value.MatSingle);

                /*var printOffsetRect = new Rect(iconRect);
                printOffsetRect.y = iconRect.yMax;
                printOffsetRect.height /= 5;
                printOffsetRect.width /= 5;
                printOffsetRect.x += printOffsetRect.width * 2;
                if (Widgets.ButtonText(printOffsetRect, "log"))
                {
                    Log.Message("iconRect size: " + iconRect.size);
                    Log.Message("offsetRect size: " + offsetRect.size);
                    Log.Message("sizeDiff: " + sizeDiff);
                    Log.Message("offset: " + offset);
                    Log.Message("centerVec: " + centerVec);
                    Log.Message("offsetRect pos: " + offsetRect.position);
                }
                var increaseOffsetXRect = new Rect(printOffsetRect);
                increaseOffsetXRect.x = printOffsetRect.xMax;
                if (Widgets.ButtonText(increaseOffsetXRect, "+ x"))
                {
                    extraOffst.x += 1f;
                }
                var decreaseOffsetXRect = new Rect(increaseOffsetXRect);
                decreaseOffsetXRect.x = increaseOffsetXRect.xMax;
                if (Widgets.ButtonText(decreaseOffsetXRect, "- x"))
                {
                    extraOffst.x -= 1f;
                }
                var increaseOffsetYRect = new Rect(printOffsetRect);
                increaseOffsetYRect.x = printOffsetRect.xMin-printOffsetRect.width;
                if (Widgets.ButtonText(increaseOffsetYRect, "+ y"))
                {
                    extraOffst.y += 1f;
                }
                var decreaseOffsetYRect = new Rect(increaseOffsetYRect);
                decreaseOffsetYRect.x = decreaseOffsetYRect.xMin-decreaseOffsetYRect.width;
                if (Widgets.ButtonText(decreaseOffsetYRect, "- y"))
                {
                    extraOffst.y -= 1f;
                }*/
            }
        }

        return iconRect;
    }
    
    //private Vector2 extraOffst = Vector2.zero;
    
    private Vector2 weaponDecorationScrollPosition;
    private float curY;
    private void DrawDecorations(Rect inRect)
    {
        inRect = inRect.ContractedBy(10);
        inRect.yMin -= 9f;

        var presetRect = new Rect(inRect)
        {
            height = inRect.height / 16
        };
        presetRect.width /= 3;
        var removeAllRect = new Rect(presetRect);
        var editPresetRect = new Rect(presetRect);
        
        presetRect.x += presetRect.width*2;
        editPresetRect.x += editPresetRect.width*1;

        inRect.yMin += presetRect.height;
        
        removeAllRect = removeAllRect.ContractedBy(3f);
        editPresetRect = editPresetRect.ContractedBy(3f);
        presetRect = presetRect.ContractedBy(3f);

        removeAllRect.x -= 2.5f;
        presetRect.x += 2.5f;
        
        if (Widgets.ButtonText(presetRect, "BEWH.Framework.ExtraDecoration.Select".Translate()))
        {
            var floatMenuOptions = new List<FloatMenuOption>();
        
            var modsettingPresets = ModSettings.ExtraDecorationPresets.Where(deco => deco.appliesTo == weapon.def.defName);
            
            foreach (var preset in modsettingPresets)
            {
                var menuOption = new FloatMenuOption(preset.name, delegate
                {
                    WeaponDecorationComp.RemoveAllDecorations();
                    WeaponDecorationComp.LoadFromPreset(preset);
                    recache = true;
                });
                floatMenuOptions.Add(menuOption);
            }
            
            foreach (var weaponDecorationPresetDef in weaponDecorationPresets.Where(deco => deco.appliesTo.Contains(weapon.def)))
            {
                var menuOption = new FloatMenuOption(weaponDecorationPresetDef.label, delegate
                {
                    WeaponDecorationComp.RemoveAllDecorations();
                    WeaponDecorationComp.LoadFromPreset(weaponDecorationPresetDef);
                    recache = true;
                });
                floatMenuOptions.Add(menuOption);
            }
            
            if (floatMenuOptions.NullOrEmpty())
            {
                var menuOptionNone = new FloatMenuOption("NoneBrackets".Translate(), null);
                floatMenuOptions.Add(menuOptionNone);
            }
        
            Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
        }

        if (Widgets.ButtonText(editPresetRect, "BEWH.Framework.ApparelMultiColor.EditPreset".Translate()))
        {
            var floatMenuOptions = new List<FloatMenuOption>();
            
            var currentPreset = GetCurrentPreset("");
            
            var modsettingPresets = ModSettings.ExtraDecorationPresets.Where(deco => deco.appliesTo == weapon.def.defName);
            
            //Delete or override existing
            foreach (var preset in modsettingPresets)
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

        if (Widgets.ButtonText(removeAllRect, "BEWH.Framework.ExtraDecoration.RemoveAllDecorations".Translate()))
        {
            WeaponDecorationComp.RemoveAllDecorations();
        }
        
        var viewRect = new Rect(inRect);
        viewRect.height -= 16f;
        viewRect.width -= 16f;
        
        var headerHeight = viewRect.height / 12;
        var decoHeight = viewRect.width / 4;
        var iconSize = new Vector2(decoHeight, decoHeight);
        
        viewRect.height = curY;
        
        var curX = viewRect.x;
        curY = inRect.y;
        Widgets.DrawMenuSection(inRect.ContractedBy(-1));
        Widgets.BeginScrollView(inRect, ref weaponDecorationScrollPosition, viewRect);
        
        var rowExpanded = false;
        
        foreach (var weaponDecoration in weaponDecorations)
        {
            var headerRect = new Rect(viewRect)
            {
                height = headerHeight,
                y = curY
            };
            curY += headerRect.height;
            headerRect = headerRect.ContractedBy(5f);
            
            Widgets.DrawMenuSection(headerRect.ContractedBy(-1));
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(headerRect, weaponDecoration.Key.label);
            Text.Anchor = TextAnchor.UpperLeft;
            
            for (var i = 0; i < weaponDecoration.Value.Count; i++)
            {
                var curDecoIsIncompatible = selectedAlternateBaseForm != null && selectedAlternateBaseForm.incompatibleWeaponDecorations.Contains(weaponDecoration.Value[i]);
                var hasDeco = WeaponDecorationComp.WeaponDecorations.ContainsKey(weaponDecoration.Value[i]);
                
                var position = new Vector2(curX, curY);
                var iconRect = new Rect(position, iconSize);
                curX += iconRect.width;
                    
                iconRect = iconRect.ContractedBy(5f);
                
                if (hasDeco)
                {
                    Widgets.DrawStrongHighlight(iconRect.ExpandedBy(3f));
                }

                var hasReq = weaponDecoration.Value[i].HasRequirements(pawn, out var reason);
                var incompatibleDeco = curDecoIsIncompatible 
                                       || (selectedAlternateBaseForm == null 
                                           && weaponDecoration.Value[i].isIncompatibleWithBaseTexture);
                
                var color = Color.white;
                var tipTooltip = weaponDecoration.Value[i].label;
                if (Mouse.IsOver(iconRect))
                {
                    color = GenUI.MouseoverColor;
                }
                if (!hasReq)
                {
                    tipTooltip += "\n" + "BEWH.Framework.DecoRequirement.RequirementNotMet".Translate() + reason;
                    color = Color.gray;
                }
                if (incompatibleDeco)
                {
                    tipTooltip += "\n" +"BEWH.Framework.WeaponDecoration.IncompatibleWithCurrentAltBase".Translate();
                    color = Color.gray;
                }
                
                GUI.color = color;
                GUI.DrawTexture(iconRect, Command.BGTexShrunk);
                GUI.color = Color.white;
                GUI.DrawTexture(iconRect, weaponDecoration.Value[i].Icon);
                TooltipHandler.TipRegion(iconRect, tipTooltip);
                
                if(hasReq && !incompatibleDeco)
                {
                    if (Widgets.ButtonInvisible(iconRect))
                    {
                        WeaponDecorationComp.AddOrRemoveDecoration(weaponDecoration.Value[i]);
                        recache = true;
                    }
                
                    if (weaponDecoration.Value[i].colourable && WeaponDecorationComp.WeaponDecorations.ContainsKey(weaponDecoration.Value[i]))
                    {
                        rowExpanded = true;
                    
                        var bottomRect = new Rect(new Vector2(iconRect.x, iconRect.yMax + 3f), iconRect.size);
                        bottomRect.height /= 3;
                        bottomRect = bottomRect.ContractedBy(2f);
                    
                        Rect colourSelection;
                        Rect colourSelectionTwo;
                        Rect colourSelectionThree;
                
                        var colorAmount = weaponDecoration.Value[i].colorAmount;
                    
                        switch (colorAmount)
                        {
                            case 1:
                                colourSelection = new Rect(bottomRect);
                        
                                PrimaryColorBox(colourSelection, weaponDecoration.Value[i]);
                                break;
                            case 2:
                                colourSelection = new Rect(bottomRect);
                                colourSelection.width /= 2;
                                colourSelectionTwo = new Rect(colourSelection)
                                {
                                    x = colourSelection.xMax
                                };

                                PrimaryColorBox(colourSelection, weaponDecoration.Value[i]);
                                SecondaryColorBox(colourSelectionTwo, weaponDecoration.Value[i]);
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

                                PrimaryColorBox(colourSelection, weaponDecoration.Value[i]);
                                SecondaryColorBox(colourSelectionTwo, weaponDecoration.Value[i]);
                                TertiaryColorBox(colourSelectionThree, weaponDecoration.Value[i]);
                                break;
                            default:
                                Log.Warning("Wrong setup in " + weaponDecoration.Value[i] + "colorAmount is more than 3 or less than 1");
                                break;
                        }
                    }
                }
                
                if (i != 0 && (i+1) % 4 == 0 || i == weaponDecoration.Value.Count - 1)
                {
                    curY += iconSize.x;
                    curX = viewRect.x;
                    if (rowExpanded)
                    {
                        curY += iconRect.height/3;
                        rowExpanded = false;
                    }
                }
            }
        }
        Widgets.EndScrollView();
    }
    
    private ExtraDecorationPreset GetCurrentPreset(string presetName)
    {
        var extraDecorationPresetParts = new List<ExtraDecorationPresetParts>();
        
        foreach (var decoration in WeaponDecorationComp.WeaponDecorations)
        {
            var presetPart = new ExtraDecorationPresetParts()
            {
                extraDecorationDefs = decoration.Key.defName,
                colour = decoration.Value.Color,
                colourTwo = decoration.Value.ColorTwo,
                colourThree = decoration.Value.ColorThree,
            };
                
            extraDecorationPresetParts.Add(presetPart);
        }

        var extraDecorationPreset = new ExtraDecorationPreset()
        {
            extraDecorationPresetParts = extraDecorationPresetParts,
            appliesTo = weapon.def.defName,
            name = presetName
        };

        return extraDecorationPreset;
    }

    private Dictionary<AlternateBaseFormDef, Material> cachedAlternateBaseForms = new Dictionary<AlternateBaseFormDef, Material>();
    private void DrawWeaponColoring(Rect inRect, Rect iconRect)
    {
        var presetRect = new Rect(iconRect)
        {
            y = iconRect.yMax
        };
        presetRect.height /= 8;
        presetRect.y += presetRect.height*1.5f;

        var editPresetRect = new Rect(presetRect);
        editPresetRect.width /= 3;
        
        var selectBaseFormRect = new Rect(editPresetRect)
        {
            x = editPresetRect.xMax
        };

        var selectPresetRect = new Rect(selectBaseFormRect)
        {
            x = selectBaseFormRect.xMax
        };
        
        editPresetRect.ContractedBy(10f);
        editPresetRect.x -= 5;
        selectBaseFormRect.ContractedBy(10f);
        selectPresetRect.ContractedBy(10f);
        selectPresetRect.x += 5;

        //Select button
        if (Widgets.ButtonText(selectPresetRect, "BEWH.Framework.ApparelMultiColor.SelectPreset".Translate()))
        {
            var list = new List<FloatMenuOption>();
            //Default Color of weapon
            var defaultMenuOption = new FloatMenuOption("BEWH.Framework.CommonKeyword.Default".Translate(), delegate
            {
                MultiColorComp.DrawColor = originalColor.col1;
                MultiColorComp.DrawColorTwo = originalColor.col2;
                MultiColorComp.DrawColorThree = originalColor.col3;
                recache = true;
                            
            }, Core40kUtils.ThreeColourPreview(originalColor.col1, originalColor.col2, originalColor.col3, 3), Color.white);
            list.Add(defaultMenuOption);
            foreach (var preset in presets.Where(p => p.appliesTo.Contains(weapon.def.defName) || p.appliesTo.Empty()))
            {
                var menuOption = new FloatMenuOption(preset.label, delegate
                {
                    MultiColorComp.DrawColor = preset.primaryColour;
                    MultiColorComp.DrawColorTwo = preset.secondaryColour;
                    MultiColorComp.DrawColorThree = preset.tertiaryColour ?? preset.secondaryColour;
                    recache = true;
                            
                }, Core40kUtils.ThreeColourPreview(preset.primaryColour, preset.secondaryColour, preset.tertiaryColour, preset.colorAmount), Color.white);
                list.Add(menuOption);
            }
                    
            foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Weapon or PresetType.All))
            {
                var menuOption = new FloatMenuOption(preset.name.CapitalizeFirst(), delegate
                {
                    MultiColorComp.DrawColor = preset.primaryColour;
                    MultiColorComp.DrawColorTwo = preset.secondaryColour;
                    MultiColorComp.DrawColorThree = preset.tertiaryColour ?? preset.secondaryColour;
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
        if (Widgets.ButtonText(editPresetRect, "BEWH.Framework.ApparelMultiColor.EditPreset".Translate()))
        {
            var list = new List<FloatMenuOption>();
                    
            //Delete or override existing
            foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Weapon or PresetType.All))
            {
                var menuOption = new FloatMenuOption(preset.name, delegate
                {
                    ModSettings.UpdatePreset(preset, MultiColorComp.DrawColor, MultiColorComp.DrawColorTwo, MultiColorComp.DrawColorThree);
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
                    primaryColour = MultiColorComp.DrawColor,
                    secondaryColour = MultiColorComp.DrawColorTwo,
                    tertiaryColour = MultiColorComp.DrawColorThree,
                    appliesToKind = PresetType.Weapon,
                };
                GUI.SetNextControlName("BEWH_Preset_Window");
                Find.WindowStack.Add( new Dialog_EditColourPresets(newColourPreset));
            }, Widgets.PlaceholderIconTex, Color.white);
            list.Add(newPreset);
                
            if (!list.NullOrEmpty())
            {
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }
        
        //Switch base texture
        if (MultiColorComp != null && Widgets.ButtonText(selectBaseFormRect, "BEWH.Framework.WeaponDecoration.AlternativeBaseTexture".Translate()))
        {
            var list = new List<FloatMenuOption>();
            var mouseOffset = UI.MousePositionOnUI;

            foreach (var alternateBaseForm in alternateBaseForms)
            {
                if (alternateBaseForm == selectedAlternateBaseForm)
                {
                    continue;
                }

                if (!cachedAlternateBaseForms.ContainsKey(alternateBaseForm) || recache)
                {
                    if (recache)
                    {
                        cachedAlternateBaseForms = new Dictionary<AlternateBaseFormDef, Material>();
                    }

                    var path = alternateBaseForm.drawnTextureIconPath;
                    var shader = alternateBaseForm.shaderType?.Shader ?? Core40kDefOf.BEWH_CutoutThreeColor.Shader;
                    var graphic = MultiColorUtils.GetGraphic<Graphic_Single>(path, shader,
                        weapon.def.graphicData.drawSize, MultiColorComp.DrawColor, MultiColorComp.DrawColorTwo,
                        MultiColorComp.DrawColorThree, weapon.Graphic.data);
                    
                    var material = graphic.MatSouth;
                    cachedAlternateBaseForms.Add(alternateBaseForm, material);
                    recache = false;
                }

                var menuOption = new FloatMenuOption(alternateBaseForm.label, delegate 
                {
                        MultiColorComp.currentAlternateBaseForm = alternateBaseForm;
                        selectedAlternateBaseForm = alternateBaseForm;
                        foreach (var weaponDecorationDef in alternateBaseForm.incompatibleWeaponDecorations)
                        {
                            if (WeaponDecorationComp.WeaponDecorations.ContainsKey(weaponDecorationDef))
                            {
                                WeaponDecorationComp.AddOrRemoveDecoration(weaponDecorationDef);
                            }
                        }
                        recache = true;
                    }, Widgets.PlaceholderIconTex, Color.white, mouseoverGuiAction: delegate(Rect rect)
                    {
                        if (!Mouse.IsOver(rect))
                        {
                            return;
                        }
                        var pictureSize = new Vector2(200, 200);
                        var mouseAttachedWindowPos = new Vector2(rect.width, rect.height);
                        mouseAttachedWindowPos.x += mouseOffset.x;
                        mouseAttachedWindowPos.y += mouseOffset.y;
                        
                        var pictureRect = new Rect(mouseAttachedWindowPos, pictureSize);

                        Find.WindowStack.ImmediateWindow(1859615242, pictureRect, WindowLayer.Super, delegate
                        {
                            Widgets.DrawMenuSection(pictureRect.AtZero());
                            Widgets.DrawTextureFitted(pictureRect.AtZero(), cachedAlternateBaseForms[alternateBaseForm].mainTexture, 1f, cachedAlternateBaseForms[alternateBaseForm]);
                        });
                    });
                
                    list.Add(menuOption);
            }
            
            if (selectedAlternateBaseForm != null)
            {
                var menuOptionDefault = new FloatMenuOption("BEWH.Framework.CommonKeyword.Default".Translate(), delegate
                {
                    MultiColorComp.currentAlternateBaseForm = null;
                    selectedAlternateBaseForm = null;
                    var decosIncompatibleWithBase = WeaponDecorationComp.WeaponDecorations.Keys
                        .Where(def => def.isIncompatibleWithBaseTexture).ToList();
                    foreach (var weaponDecorationDef in decosIncompatibleWithBase)
                    {
                        WeaponDecorationComp.AddOrRemoveDecoration(weaponDecorationDef);
                    }
                    recache = true;
                }, Widgets.PlaceholderIconTex, Color.white, mouseoverGuiAction: delegate(Rect rect)
                {
                    if (!Mouse.IsOver(rect))
                    {
                        return;
                    }
                    var pictureSize = new Vector2(200, 200);
                    var mouseAttachedWindowPos = new Vector2(rect.width, rect.height);
                    mouseAttachedWindowPos.x += mouseOffset.x;
                    mouseAttachedWindowPos.y += mouseOffset.y;
                        
                    var pictureRect = new Rect(mouseAttachedWindowPos, pictureSize);
                    var graphic = MultiColorComp.GetSingleGraphic(true);
                    
                    Find.WindowStack.ImmediateWindow(1859615242, pictureRect, WindowLayer.Super, delegate
                    {
                        Widgets.DrawMenuSection(pictureRect.AtZero());
                        Widgets.DrawTextureFitted(pictureRect.AtZero(), graphic.MatSingle.mainTexture, 1f, graphic.MatSingle);
                    });
                });
                list.Add(menuOptionDefault);
            }
            if (list.NullOrEmpty())
            {
                var menuOptionNone = new FloatMenuOption("NoneBrackets".Translate(), null);
                list.Add(menuOptionNone);
            }
            
            Find.WindowStack.Add(new FloatMenu(list));
        }
        
        var colorRects = inRect.TakeBottomPart(inRect.height / 4f);;
        Rect colorOneRect;
        Rect colorTwoRect;
        Rect colorThreeRect;

        switch (weaponMaskAmount)
        {
            case 1:
                colorOneRect = new Rect(colorRects);
                colorOneRect.y = colorRects.yMax - colorRects.height / 2 - colorOneRect.height / 2;
                colorOneRect = colorOneRect.ContractedBy(10);
                
                PrimaryColorBox(colorOneRect);
                break;
            case 2:
                colorOneRect = new Rect(colorRects);
                colorOneRect.width /= 2;
                colorOneRect.y = colorRects.yMax - colorRects.height / 2 - colorOneRect.height / 2;
                colorTwoRect = new Rect(colorOneRect)
                {
                    x = colorOneRect.xMax
                };
                colorOneRect = colorOneRect.ContractedBy(10);
                colorTwoRect = colorTwoRect.ContractedBy(10);
                
                PrimaryColorBox(colorOneRect);
                SecondaryColorBox(colorTwoRect);
                break;
            case 3:
                colorOneRect = new Rect(colorRects);
                colorOneRect.width /= 3;
                colorOneRect.y = colorRects.yMax - colorRects.height / 2 - colorOneRect.height / 2;
                        
                colorTwoRect = new Rect(colorOneRect)
                {
                    x = colorOneRect.xMax
                };

                colorThreeRect = new Rect(colorTwoRect)
                {
                    x = colorTwoRect.xMax
                };
                
                colorOneRect = colorOneRect.ContractedBy(10);
                colorTwoRect = colorTwoRect.ContractedBy(10);
                colorThreeRect = colorThreeRect.ContractedBy(10);
                
                PrimaryColorBox(colorOneRect);
                SecondaryColorBox(colorTwoRect);
                TertiaryColorBox(colorThreeRect);
                break;
        }
    }
    
    //Multicolor color boxes
    private void PrimaryColorBox(Rect colorOneRect)
    {
        Widgets.DrawMenuSection(colorOneRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorOneRect, MultiColorComp.DrawColor);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorOneRect, "BEWH.Framework.ApparelMultiColor.PrimaryColor".Translate());
        TooltipHandler.TipRegion(colorOneRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorOneRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( MultiColorComp.DrawColor, ( newColour ) =>
            {
                MultiColorComp.DrawColor = newColour;
                recache = true;
            } ) );
        }
    }
    
    private void SecondaryColorBox(Rect colorTwoRect)
    {
        Widgets.DrawMenuSection(colorTwoRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorTwoRect, MultiColorComp.DrawColorTwo);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorTwoRect, "BEWH.Framework.ApparelMultiColor.SecondaryColor".Translate());
        TooltipHandler.TipRegion(colorTwoRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorTwoRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( MultiColorComp.DrawColorTwo, ( newColour ) =>
            {
                MultiColorComp.DrawColorTwo = newColour;
                recache = true;
            } ) );
        }
    }
    
    private void TertiaryColorBox(Rect colorThreeRect)
    {
        Widgets.DrawMenuSection(colorThreeRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorThreeRect, MultiColorComp.DrawColorThree);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorThreeRect, "BEWH.Framework.ApparelMultiColor.TertiaryColor".Translate());
        TooltipHandler.TipRegion(colorThreeRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorThreeRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( MultiColorComp.DrawColorThree, ( newColour ) =>
            {
                MultiColorComp.DrawColorThree = newColour;
                recache = true;
            } ) );
        }
    }
    
    //Attachments color boxes
    private void PrimaryColorBox(Rect colourSelection, WeaponDecorationDef weaponDecorationDef)
    {
        colourSelection = colourSelection.ContractedBy(2f);
        Widgets.DrawMenuSection(colourSelection);
        colourSelection = colourSelection.ContractedBy(1f);
        Widgets.DrawRectFast(colourSelection, WeaponDecorationComp.WeaponDecorations[weaponDecorationDef].Color);
        TooltipHandler.TipRegion(colourSelection, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        if (Widgets.ButtonInvisible(colourSelection))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( WeaponDecorationComp.WeaponDecorations[weaponDecorationDef].Color, ( newColour ) =>
            {
                recache = true;
                WeaponDecorationComp.UpdateDecorationColourOne(weaponDecorationDef, newColour);
            } ) );
        }
    }
    
    private void SecondaryColorBox(Rect colourSelectionTwo, WeaponDecorationDef weaponDecorationDef)
    {
        colourSelectionTwo = colourSelectionTwo.ContractedBy(2f);
        Widgets.DrawMenuSection(colourSelectionTwo);
        colourSelectionTwo = colourSelectionTwo.ContractedBy(1f);
        Widgets.DrawRectFast(colourSelectionTwo, WeaponDecorationComp.WeaponDecorations[weaponDecorationDef].ColorTwo);
        TooltipHandler.TipRegion(colourSelectionTwo, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        if (Widgets.ButtonInvisible(colourSelectionTwo))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( WeaponDecorationComp.WeaponDecorations[weaponDecorationDef].ColorTwo, ( newColour ) =>
            {
                recache = true;
                WeaponDecorationComp.UpdateDecorationColourTwo(weaponDecorationDef, newColour);
            } ) );
        }
    }
    
    private void TertiaryColorBox(Rect colourSelectionThree, WeaponDecorationDef weaponDecorationDef)
    {
        colourSelectionThree = colourSelectionThree.ContractedBy(2f);
        Widgets.DrawMenuSection(colourSelectionThree);
        colourSelectionThree = colourSelectionThree.ContractedBy(1f);
        Widgets.DrawRectFast(colourSelectionThree, WeaponDecorationComp.WeaponDecorations[weaponDecorationDef].ColorThree);
        TooltipHandler.TipRegion(colourSelectionThree, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        if (Widgets.ButtonInvisible(colourSelectionThree))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( WeaponDecorationComp.WeaponDecorations[weaponDecorationDef].ColorThree, ( newColour ) =>
            {
                recache = true;
                WeaponDecorationComp.UpdateDecorationColourThree(weaponDecorationDef, newColour);
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
        
        base.Close(doCloseSound);
    }

    private void Reset()
    {
        MultiColorComp?.Reset();
        WeaponDecorationComp?.Reset();
        recache = true;
        
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void Accept()
    {
        MultiColorComp?.Notify_GraphicChanged();
        WeaponDecorationComp?.Notify_GraphicChanged();
        MultiColorComp?.SetOriginals();
        WeaponDecorationComp?.SetOriginals();
    }
}