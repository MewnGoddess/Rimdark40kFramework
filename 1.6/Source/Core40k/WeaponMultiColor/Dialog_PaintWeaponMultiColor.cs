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

    private WeaponMultiColor weapon;

    private int weaponMaskAmount = -1;
    private int WeaponMaskAmount
    {
        get
        {
            if (weaponMaskAmount == -1)
            {
                weaponMaskAmount = weapon.def.HasModExtension<DefModExtension_WeaponMultiColor>() ? weapon.def.GetModExtension<DefModExtension_WeaponMultiColor>().colorMaskAmount : 1;
            }

            return weaponMaskAmount;
        }
    }
    
    private bool recache = true;
    
    public Dialog_PaintWeaponMultiColor()
    {
    }

    public Dialog_PaintWeaponMultiColor(Pawn pawn, WeaponMultiColor weaponMultiColor)
    {
        this.pawn = pawn;
            
        presets = DefDatabase<ColourPresetDef>.AllDefs.ToList();

        weapon = weaponMultiColor;

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
            var graphic = MultiColorUtils.GetGraphic<Graphic_Single>(path, shader, weapon.def.graphicData.drawSize, weapon.DrawColor, weapon.DrawColorTwo, weapon.DrawColorThree, weapon.Graphic.data, null);
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
            foreach (var preset in presets.Where(p => p.appliesTo.Contains(weapon.def.defName) || p.appliesTo.Empty()))
            {
                var menuOption = new FloatMenuOption(preset.label, delegate
                {
                    weapon.DrawColor = preset.primaryColour;
                    weapon.SetSecondaryColor(preset.secondaryColour);
                    weapon.SetTertiaryColor(preset.tertiaryColour);
                    recache = true;
                            
                }, Core40kUtils.ThreeColourPreview(preset.primaryColour, preset.secondaryColour, preset.tertiaryColour, preset.colorAmount), Color.white);
                list.Add(menuOption);
            }
                    
            foreach (var preset in ModSettings.ColourPresets)
            {
                var menuOption = new FloatMenuOption(preset.name.CapitalizeFirst(), delegate
                {
                    weapon.DrawColor = preset.primaryColour;
                    weapon.SetSecondaryColor(preset.secondaryColour);
                    weapon.SetTertiaryColor(preset.tertiaryColour);
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
            foreach (var preset in ModSettings.ColourPresets)
            {
                var menuOption = new FloatMenuOption(preset.name, delegate
                {
                    ModSettings.UpdatePreset(preset, weapon.DrawColor, weapon.DrawColorTwo, weapon.DrawColorThree);
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
                    primaryColour = weapon.DrawColor,
                    secondaryColour = weapon.DrawColorTwo,
                    tertiaryColour = weapon.DrawColorThree
                };
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

        switch (WeaponMaskAmount)
        {
            case 1:
                colorRects = inRect.TakeBottomPart(inRect.height / 2f);
                colorOneRect = new Rect(colorRects);
                colorOneRect.y = colorRects.yMax - colorRects.height / 2 - colorOneRect.height / 2;
                colorOneRect = colorOneRect.ContractedBy(10);
                
                Widgets.DrawMenuSection(colorOneRect.ContractedBy(-1));
                Widgets.DrawRectFast(colorOneRect, weapon.DrawColor);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(colorOneRect, "BEWH.Framework.ApparelMultiColor.PrimaryColor".Translate());
                TooltipHandler.TipRegion(colorOneRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                if (Widgets.ButtonInvisible(colorOneRect))
                {
                    Find.WindowStack.Add( new Dialog_ColourPicker( weapon.DrawColor, ( newColour ) =>
                    {
                        weapon.DrawColor = newColour;
                        recache = true;
                    } ) );
                }
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
                
                Widgets.DrawMenuSection(colorOneRect.ContractedBy(-1));
                Widgets.DrawRectFast(colorOneRect, weapon.DrawColor);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(colorOneRect, "BEWH.Framework.ApparelMultiColor.PrimaryColor".Translate());
                TooltipHandler.TipRegion(colorOneRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                if (Widgets.ButtonInvisible(colorOneRect))
                {
                    Find.WindowStack.Add( new Dialog_ColourPicker( weapon.DrawColor, ( newColour ) =>
                    {
                        weapon.DrawColor = newColour;
                        recache = true;
                    } ) );
                }
                
                Widgets.DrawMenuSection(colorTwoRect.ContractedBy(-1));
                Widgets.DrawRectFast(colorTwoRect, weapon.DrawColorTwo);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(colorTwoRect, "BEWH.Framework.ApparelMultiColor.SecondaryColor".Translate());
                TooltipHandler.TipRegion(colorTwoRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                if (Widgets.ButtonInvisible(colorTwoRect))
                {
                    Find.WindowStack.Add( new Dialog_ColourPicker( weapon.DrawColorTwo, ( newColour ) =>
                    {
                        weapon.SetSecondaryColor(newColour);
                        recache = true;
                    } ) );
                }
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
                
                Widgets.DrawMenuSection(colorOneRect.ContractedBy(-1));
                Widgets.DrawRectFast(colorOneRect, weapon.DrawColor);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(colorOneRect, "BEWH.Framework.ApparelMultiColor.PrimaryColor".Translate());
                TooltipHandler.TipRegion(colorOneRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                if (Widgets.ButtonInvisible(colorOneRect))
                {
                    Find.WindowStack.Add( new Dialog_ColourPicker( weapon.DrawColor, ( newColour ) =>
                    {
                        weapon.DrawColor = newColour;
                        recache = true;
                    } ) );
                }
                
                Widgets.DrawMenuSection(colorTwoRect.ContractedBy(-1));
                Widgets.DrawRectFast(colorTwoRect, weapon.DrawColorTwo);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(colorTwoRect, "BEWH.Framework.ApparelMultiColor.SecondaryColor".Translate());
                TooltipHandler.TipRegion(colorTwoRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                if (Widgets.ButtonInvisible(colorTwoRect))
                {
                    Find.WindowStack.Add( new Dialog_ColourPicker( weapon.DrawColorTwo, ( newColour ) =>
                    {
                        weapon.SetSecondaryColor(newColour);
                        recache = true;
                    } ) );
                }
                
                Widgets.DrawMenuSection(colorThreeRect.ContractedBy(-1));
                Widgets.DrawRectFast(colorThreeRect, weapon.DrawColorThree);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(colorThreeRect, "BEWH.Framework.ApparelMultiColor.TertiaryColor".Translate());
                TooltipHandler.TipRegion(colorThreeRect, "BEWH.Framework.ApparelMultiColor.ChooseCustomColour".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                if (Widgets.ButtonInvisible(colorThreeRect))
                {
                    Find.WindowStack.Add( new Dialog_ColourPicker( weapon.DrawColorThree, ( newColour ) =>
                    {
                        weapon.SetTertiaryColor(newColour);
                        recache = true;
                    } ) );
                }
                break;
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
        weapon.Reset();
        recache = true;
        
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void Accept()
    {
        weapon.Notify_ColorChanged();
        weapon.SetOriginals();
    }
}