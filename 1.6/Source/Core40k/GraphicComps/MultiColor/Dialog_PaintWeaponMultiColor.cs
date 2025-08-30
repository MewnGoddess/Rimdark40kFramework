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
        
    private static Core40kModSettings modSettings = null;

    public static Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();

    private Material cachedMaterial = null;

    private ThingWithComps weapon;
    private CompMultiColor multiColor => weapon.GetComp<CompMultiColor>();

    private int weaponMaskAmount => multiColor.Props.colorMaskAmount;
    
    private bool recache = true;
    
    private (Color col1, Color col2, Color col3) originalColor = (Color.white, Color.white, Color.white);
    
    public Dialog_PaintWeaponMultiColor()
    {
    }

    public Dialog_PaintWeaponMultiColor(Pawn pawn, ThingWithComps weaponMultiColor)
    {
        this.pawn = pawn;
            
        presets = DefDatabase<ColourPresetDef>.AllDefs.Where(preset => preset.appliesToKind is PresetType.Weapon or PresetType.All).ToList();

        weapon = weaponMultiColor;

        originalColor = (multiColor.Props?.defaultPrimaryColor ?? (weapon.def.MadeFromStuff ? weapon.def.GetColorForStuff(weapon.Stuff) : Color.white), multiColor.Props?.defaultSecondaryColor ?? Color.white, multiColor.Props?.defaultTertiaryColor ?? Color.white);

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
        
        DrawWeaponColoring(rect2);
            
        DrawBottomButtons(inRect);
    }

    private void DrawWeaponColoring(Rect inRect)
    {
        var iconRect = inRect.TakeTopPart(inRect.height / 2f);
        iconRect.width = iconRect.height;
        iconRect.x += inRect.width / 2 - iconRect.width / 2;

        iconRect = iconRect.ContractedBy(1);

        if (cachedMaterial == null || recache)
        {
            var path = weapon.def.graphicData.texPath;
            var shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;
            var graphic = MultiColorUtils.GetGraphic<Graphic_Single>(path, shader, weapon.def.graphicData.drawSize, multiColor.DrawColor, multiColor.DrawColorTwo, multiColor.DrawColorThree, weapon.Graphic.data, null);
            var material = graphic.MatSouth;
            cachedMaterial = material;
            recache = false;
        }
        
        Widgets.DrawMenuSection(iconRect.ContractedBy(-1));
        Graphics.DrawTexture(iconRect, cachedMaterial.mainTexture, cachedMaterial);
        
        var presetRect = new Rect(iconRect)
        {
            y = iconRect.yMax
        };
        presetRect.height /= 3;
        presetRect.width *= 2;
        presetRect.x -= presetRect.width / 4;
        presetRect.y += presetRect.height / 3;
        
        var editPresetRect = new Rect(presetRect);
        editPresetRect.width /= 2;
        editPresetRect = editPresetRect.ContractedBy(30);
        
        var selectPresetRect = new Rect(presetRect);
        selectPresetRect.width /= 2;
        selectPresetRect.x += selectPresetRect.width;
        selectPresetRect = selectPresetRect.ContractedBy(30);
        
        //Select button
        if (Widgets.ButtonText(selectPresetRect, "BEWH.Framework.ApparelMultiColor.SelectPreset".Translate()))
        {
            var list = new List<FloatMenuOption>();
            //Default Color of weapon
            var defaultMenuOption = new FloatMenuOption("BEWH.Framework.CommonKeyword.Default".Translate(), delegate
            {
                multiColor.DrawColor = originalColor.col1;
                multiColor.DrawColorTwo = originalColor.col2;
                multiColor.DrawColorThree = originalColor.col3;
                recache = true;
                            
            }, Core40kUtils.ThreeColourPreview(originalColor.col1, originalColor.col2, originalColor.col3, 3), Color.white);
            list.Add(defaultMenuOption);
            foreach (var preset in presets.Where(p => p.appliesTo.Contains(weapon.def.defName) || p.appliesTo.Empty()))
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
                    
            foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Weapon or PresetType.All))
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
        if (Widgets.ButtonText(editPresetRect, "BEWH.Framework.ApparelMultiColor.EditPreset".Translate()))
        {
            var list = new List<FloatMenuOption>();
                    
            //Delete or override existing
            foreach (var preset in ModSettings.ColourPresets.Where(preset => preset.appliesToKind is PresetType.Weapon or PresetType.All))
            {
                var menuOption = new FloatMenuOption(preset.name, delegate
                {
                    ModSettings.UpdatePreset(preset, multiColor.DrawColor, multiColor.DrawColorTwo, multiColor.DrawColorThree);
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
                    primaryColour = multiColor.DrawColor,
                    secondaryColour = multiColor.DrawColorTwo,
                    tertiaryColour = multiColor.DrawColorThree,
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

        Rect colorRects;
        Rect colorOneRect;
        Rect colorTwoRect;
        Rect colorThreeRect;

        switch (weaponMaskAmount)
        {
            case 1:
                colorRects = inRect.TakeBottomPart(inRect.height / 2f);
                colorOneRect = new Rect(colorRects);
                colorOneRect.y = colorRects.yMax - colorRects.height / 2 - colorOneRect.height / 2;
                colorOneRect = colorOneRect.ContractedBy(10);
                
                PrimaryColorBox(colorOneRect);
                break;
            case 2:
                colorRects = inRect.TakeBottomPart(inRect.height / 2f);
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
                colorRects = inRect.TakeBottomPart(inRect.height / 2f);
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
    
    private void PrimaryColorBox(Rect colorOneRect)
    {
        Widgets.DrawMenuSection(colorOneRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorOneRect, multiColor.DrawColor);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorOneRect, "BEWH.Framework.ApparelMultiColor.PrimaryColor".Translate());
        TooltipHandler.TipRegion(colorOneRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorOneRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( multiColor.DrawColor, ( newColour ) =>
            {
                multiColor.DrawColor = newColour;
                recache = true;
            } ) );
        }
    }
    
    private void SecondaryColorBox(Rect colorTwoRect)
    {
        Widgets.DrawMenuSection(colorTwoRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorTwoRect, multiColor.DrawColorTwo);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorTwoRect, "BEWH.Framework.ApparelMultiColor.SecondaryColor".Translate());
        TooltipHandler.TipRegion(colorTwoRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorTwoRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( multiColor.DrawColorTwo, ( newColour ) =>
            {
                multiColor.DrawColorTwo = newColour;
                recache = true;
            } ) );
        }
    }
    
    private void TertiaryColorBox(Rect colorThreeRect)
    {
        Widgets.DrawMenuSection(colorThreeRect.ContractedBy(-1));
        Widgets.DrawRectFast(colorThreeRect, multiColor.DrawColorThree);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(colorThreeRect, "BEWH.Framework.ApparelMultiColor.TertiaryColor".Translate());
        TooltipHandler.TipRegion(colorThreeRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(colorThreeRect))
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( multiColor.DrawColorThree, ( newColour ) =>
            {
                multiColor.DrawColorThree = newColour;
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
        
        base.Close(doCloseSound);
    }

    private void Reset()
    {
        multiColor.Reset();
        recache = true;
        
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void Accept()
    {
        multiColor.Notify_GraphicChanged();
        multiColor.SetOriginals();
    }
}