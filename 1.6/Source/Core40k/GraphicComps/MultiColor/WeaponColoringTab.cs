using System;
using System.Collections.Generic;
using System.Linq;
using ColourPicker;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class WeaponColoringTab : CustomizerTabDrawer
{
    private static Core40kModSettings modSettings = null;
    public static Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();
    
    private bool recache = true;
    private Dictionary<ThingDef, int> apparelColorMaskPageNumber = new ();
    
    private Dictionary<ThingDef, List<MaskDef>> masks = new ();

    private List<ColourPresetDef> presets;
    
    private Dictionary<(ThingDef, MaskDef), Material> cachedMaterials = new ();

    private ThingWithComps weapon = null;
    private CompMultiColor MultiColor = null;
    
    private (Color col1, Color col2, Color col3) originalColor = (Color.white, Color.white, Color.white);

    public override void Setup(Pawn pawn)
    {
        presets = DefDatabase<ColourPresetDef>.AllDefs.Where(preset => preset.appliesToKind is PresetType.Weapon or PresetType.All).ToList();
        
        var masksTemp = DefDatabase<MaskDef>.AllDefs.Where(def => def.appliesToKind is AppliesToKind.Thing or AppliesToKind.All).ToList();

        weapon = pawn.equipment.Primary;
        
        MultiColor = weapon.GetComp<CompMultiColor>();
        MultiColor.SetOriginals();
        
        originalColor = (MultiColor?.Props?.defaultPrimaryColor ?? (weapon.def.MadeFromStuff ? weapon.def.GetColorForStuff(weapon.Stuff) : Color.white), MultiColor?.Props?.defaultSecondaryColor ?? Color.white, MultiColor?.Props?.defaultTertiaryColor ?? Color.white);
        
        var masksForItem = masksTemp.Where(mask => mask.appliesTo.Contains(weapon.def.defName) || mask.appliesToKind == AppliesToKind.All).ToList();

        if (masksForItem.Any())
        {
            masksForItem.SortBy(def => def.sortOrder);
            masks.Add(weapon.def, masksForItem);
        }
            
        apparelColorMaskPageNumber.Add(weapon.def, 0);
    }

    public override void DrawTab(Rect rect, Pawn pawn, ref Vector2 apparelColorScrollPosition)
    {
        var viewRect = new Rect(rect.x, rect.y, rect.width, rect.height);
        var curY = rect.y;

        //Item name
        var nameRect = new Rect(rect.x, curY, viewRect.width, 30f);
        nameRect.width /= 2;
        nameRect.x += nameRect.width / 2;
        Widgets.DrawMenuSection(nameRect);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(nameRect, weapon.LabelCap);
        Text.Anchor = TextAnchor.UpperLeft;

        //Select button
        var selectPresetRect = new Rect(rect.x, curY, viewRect.width, 30f);
        selectPresetRect.width /= 5;
        selectPresetRect.x = nameRect.xMax + nameRect.width / 20;
        if (Widgets.ButtonText(selectPresetRect, "BEWH.Framework.Customization.ColorPreset".Translate()))
        {
            var list = new List<FloatMenuOption>();
            var defaultMenuOption = new FloatMenuOption("BEWH.Framework.CommonKeyword.Default".Translate(), delegate
            {
                MultiColor.DrawColor = originalColor.col1;
                MultiColor.DrawColorTwo = originalColor.col2;
                MultiColor.DrawColorThree = originalColor.col3;
                recache = true;
                            
            }, Core40kUtils.ThreeColourPreview(originalColor.col1, originalColor.col2, originalColor.col3, 3), Color.white);
            list.Add(defaultMenuOption);
            foreach (var preset in presets.Where(p => p.appliesTo.Contains(weapon.def.defName) || p.appliesTo.Empty()))
            {
                var menuOption = new FloatMenuOption(preset.label, delegate
                    {
                        MultiColor.DrawColor = preset.primaryColour;
                        MultiColor.DrawColorTwo = preset.secondaryColour;
                        MultiColor.DrawColorThree = preset.tertiaryColour ?? preset.secondaryColour;
                        recache = true;
                    },
                    Core40kUtils.ThreeColourPreview(preset.primaryColour, preset.secondaryColour, preset.tertiaryColour,
                        preset.colorAmount), Color.white);
                list.Add(menuOption);
            }
    
            foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Weapon or PresetType.All))
            {
                var menuOption = new FloatMenuOption(preset.name.CapitalizeFirst(), delegate
                    {
                        MultiColor.DrawColor = preset.primaryColour;
                        MultiColor.DrawColorTwo = preset.secondaryColour;
                        MultiColor.DrawColorThree = preset.tertiaryColour ?? preset.secondaryColour;
                        recache = true;
                    },
                    Core40kUtils.ThreeColourPreview(preset.primaryColour, preset.secondaryColour, preset.tertiaryColour,
                        3), Color.white);
                list.Add(menuOption);
            }

            if (!list.NullOrEmpty()) Find.WindowStack.Add(new FloatMenu(list));
        }

        //Save button
        var savePresetRect = new Rect(rect.x, curY, viewRect.width, 30f);
        savePresetRect.width /= 5;
        savePresetRect.x = nameRect.xMin - savePresetRect.width - nameRect.width / 20;
        if (Widgets.ButtonText(savePresetRect, "BEWH.Framework.Customization.EditPreset".Translate()))
        {
            var list = new List<FloatMenuOption>();

            //Delete or override existing
            foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Weapon or PresetType.All))
            {
                var menuOption = new FloatMenuOption(preset.name,
                    delegate
                    {
                        Find.WindowStack.Add(new Dialog_ConfirmColorPresetOverride(preset, MultiColor.DrawColor,
                            MultiColor.DrawColorTwo, MultiColor.DrawColorThree));
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
                    primaryColour = MultiColor.DrawColor,
                    secondaryColour = MultiColor.DrawColorTwo,
                    tertiaryColour = MultiColor.DrawColorThree,
                    appliesToKind = PresetType.Weapon
                };
                Find.WindowStack.Add(new Dialog_EditColourPresets(newColourPreset));
            }, Widgets.PlaceholderIconTex, Color.white);
            list.Add(newPreset);

            if (!list.NullOrEmpty()) Find.WindowStack.Add(new FloatMenu(list));
        }
        
        curY += nameRect.height + 3f;
        var itemRect = new Rect(rect.x, curY, viewRect.width, 76f);

        var height = Math.Abs(viewRect.height - nameRect.height);
        
        var colorOneRect = new Rect(itemRect)
        {
            x = itemRect.xMin,
            y = itemRect.yMin + 2f,
            height = height
        };
        Rect colorTwoRect;
        
        var colAmount = MultiColor.Props.colorMaskAmount;
        if (MultiColor.currentAlternateBaseForm != null)
        {
            colAmount = MultiColor.currentAlternateBaseForm.colorAmount;
        }
        if (MultiColor.MaskDef is { setsNull: false })
        {
            colAmount = MultiColor.MaskDef.colorAmount;
        }
        
        switch (colAmount)
        {
            case 1:
                colorOneRect = colorOneRect.ContractedBy(3);
                PrimaryColorBox(colorOneRect);
                break;
            case 2:
                colorOneRect.height /= 2;
                colorTwoRect = new Rect(colorOneRect)
                {
                    y = colorOneRect.yMax
                };
                        
                colorOneRect = colorOneRect.ContractedBy(3);
                PrimaryColorBox(colorOneRect);
                        
                colorTwoRect = colorTwoRect.ContractedBy(3);
                SecondaryColorBox(colorTwoRect);
                break;
            case 3:
                colorOneRect.height /= 3;
                colorTwoRect = new Rect(colorOneRect)
                {
                    y = colorOneRect.yMax
                };
                var colorThreeRect = new Rect(colorTwoRect)
                {
                    y = colorTwoRect.yMax
                };
                        
                colorOneRect = colorOneRect.ContractedBy(3);
                PrimaryColorBox(colorOneRect);
                        
                colorTwoRect = colorTwoRect.ContractedBy(3);
                SecondaryColorBox(colorTwoRect);
                        
                colorThreeRect = colorThreeRect.ContractedBy(3);
                TertiaryColorBox(colorThreeRect);
                break;
            default:
                Log.Warning("Wrong setup in " + weapon + "colorAmount is more than 3 or less than 1");
                break;
        }
        
        //Mask Stuff
        if (masks.ContainsKey(weapon.def) && masks[weapon.def].Count > 1)
        {
            var maskRect = new Rect(itemRect)
            {
                y = colorOneRect.yMax + 3f
            };
            maskRect.height = maskRect.width / 4;
            var arrowsEnabled = masks[weapon.def].Count > 4;
            if (arrowsEnabled) maskRect.height += maskRect.height / 5;
            maskRect = maskRect.ContractedBy(3);
            Widgets.DrawMenuSection(maskRect.ContractedBy(-1));
            var posRect = new Rect(maskRect);
            posRect.width /= 4;
            posRect.height = posRect.width;

            var maskDefs = new List<MaskDef>();

            if (MultiColor.currentAlternateBaseForm != null)
                maskDefs.AddRange(masks[weapon.def].Where(maskDef =>
                    !MultiColor.currentAlternateBaseForm.incompatibleMaskDefs.Contains(maskDef)));
            else
                maskDefs = masks[weapon.def];

            var num = maskDefs.Count - apparelColorMaskPageNumber[weapon.def] * 4;
            num = num > 4 ? 4 : num;

            //Might null error at maskDefs.Count 0?
            var curPageMasks = maskDefs.GetRange(apparelColorMaskPageNumber[weapon.def] * 4, num);

            for (var i = 0; i < curPageMasks.Count; i++)
            {
                var curPosRect = new Rect(posRect);
                curPosRect.x += curPosRect.width * i;
                curPosRect = curPosRect.ContractedBy(15);
                if (!cachedMaterials.ContainsKey((weapon.def, curPageMasks[i])) || recache)
                {
                    if (recache)
                    {
                        cachedMaterials = new Dictionary<(ThingDef, MaskDef), Material>();
                    }
                    
                    var bodyType = weapon.def.GetModExtension<DefModExtension_ForcesBodyType>()?.forcedBodyType ?? pawn.story.bodyType;

                    var alternatePath = MultiColor.currentAlternateBaseForm?.drawnTextureIconPath;
                    var usedPath = alternatePath.NullOrEmpty() ? weapon.def.graphicData.texPath : alternatePath;

                    var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
                    var maskPath = curPageMasks[i]?.maskPath;
                    if (curPageMasks[i] != null && curPageMasks[i].useBodyTypes)
                    {
                        maskPath += "_" + bodyType.defName;
                    }
                    
                    var graphic = MultiColorUtils.GetGraphic<Graphic_Single>(usedPath, shader,
                        weapon.def.graphicData.drawSize, MultiColor.DrawColor, MultiColor.DrawColorTwo,
                        MultiColor.DrawColorThree, null, maskPath);
                    var material = graphic.MatSouth;
                    
                    cachedMaterials.Add((weapon.def, curPageMasks[i]), material);
                    recache = false;
                }

                if (MultiColor.MaskDef == curPageMasks[i]) Widgets.DrawStrongHighlight(curPosRect.ExpandedBy(6f));

                Widgets.DrawMenuSection(curPosRect.ContractedBy(-1));

                var windowMax = rect.yMax + apparelColorScrollPosition.y;
                var windowMin = rect.yMin + apparelColorScrollPosition.y;
                if (curPosRect.yMin > windowMin && curPosRect.yMax < windowMax)
                    Graphics.DrawTexture(curPosRect, cachedMaterials[(weapon.def, curPageMasks[i])].mainTexture,
                        cachedMaterials[(weapon.def, curPageMasks[i])]);

                TooltipHandler.TipRegion(curPosRect, curPageMasks[i].label);

                Widgets.DrawHighlightIfMouseover(curPosRect);
                if (Widgets.ButtonInvisible(curPosRect)) MultiColor.MaskDef = curPageMasks[i];
            }

            if (arrowsEnabled)
            {
                var arrowBack = new Rect(maskRect)
                {
                    height = maskRect.height / 5,
                    width = maskRect.height / 5
                };
                arrowBack.y = maskRect.yMax - arrowBack.height;

                if (apparelColorMaskPageNumber[weapon.def] > 0)
                {
                    if (Widgets.ButtonInvisible(arrowBack)) apparelColorMaskPageNumber[weapon.def]--;
                    arrowBack = arrowBack.ContractedBy(5);
                    Widgets.DrawTextureFitted(arrowBack, Core40kUtils.ScrollBackwardIcon, 1);
                }

                if (apparelColorMaskPageNumber[weapon.def] < Math.Ceiling((float)masks[weapon.def].Count / 4) - 1)
                {
                    var arrowForward = new Rect(arrowBack)
                    {
                        x = maskRect.xMax - arrowBack.width
                    };
                    if (Widgets.ButtonInvisible(arrowForward)) apparelColorMaskPageNumber[weapon.def]++;
                    arrowForward = arrowForward.ContractedBy(5);
                    Widgets.DrawTextureFitted(arrowForward, Core40kUtils.ScrollForwardIcon, 1);
                }
            }
        }
    }
    
    private void PrimaryColorBox(Rect colorOneRect)
    {
        Widgets.DrawMenuSection(colorOneRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorOneRect, MultiColor.DrawColor);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorOneRect, "BEWH.Framework.Customization.PrimaryColor".Translate());
        TooltipHandler.TipRegion(colorOneRect, "BEWH.Framework.Customization.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorOneRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( MultiColor.DrawColor, ( newColour ) =>
            {
                MultiColor.DrawColor = newColour;
                recache = true;
            } ) );
        }
    }
    
    private void SecondaryColorBox(Rect colorTwoRect)
    {
        Widgets.DrawMenuSection(colorTwoRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorTwoRect, MultiColor.DrawColorTwo);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorTwoRect, "BEWH.Framework.Customization.SecondaryColor".Translate());
        TooltipHandler.TipRegion(colorTwoRect, "BEWH.Framework.Customization.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorTwoRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( MultiColor.DrawColorTwo, ( newColour ) =>
            {
                MultiColor.DrawColorTwo = newColour;
                recache = true;
            } ) );
        }
    }
    
    private void TertiaryColorBox(Rect colorThreeRect)
    {
        Widgets.DrawMenuSection(colorThreeRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorThreeRect, MultiColor.DrawColorThree);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorThreeRect, "BEWH.Framework.Customization.TertiaryColor".Translate());
        TooltipHandler.TipRegion(colorThreeRect, "BEWH.Framework.Customization.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorThreeRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( MultiColor.DrawColorThree, ( newColour ) =>
            {
                MultiColor.DrawColorThree = newColour;
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
        MultiColor.SetOriginals();
        MultiColor.Notify_GraphicChanged();
    }
    
    public override void OnReset(Pawn pawn)
    {
        MultiColor.Reset();
    }
}