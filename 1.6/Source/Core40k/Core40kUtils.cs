using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Verse;

namespace Core40k;

[StaticConstructorOnStartup]
public static class Core40kUtils
{
    public const string MankindsFinestPackageId = "Phonicmas.RimDark.MankindsFinest";
        
    public static readonly Texture2D FlippedIconTex = ContentFinder<Texture2D>.Get("UI/Decoration/flipIcon");
    public static readonly Texture2D ScrollForwardIcon = ContentFinder<Texture2D>.Get ("UI/Misc/ScrollForwardIcon");
    public static readonly Texture2D ScrollBackwardIcon = ContentFinder<Texture2D>.Get ("UI/Misc/ScrollBackwardIcon");
    
    public static readonly Graphic_Multi EmptyMultiGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>("UI/EmptyImage");
    
    private static Core40kModSettings modSettings = null;
    public static Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();

    public static readonly Color RequirementMetColour = Color.white;
    public static readonly Color RequirementNotMetColour = new Color(1f, 0.0f, 0.0f, 0.8f);
    
    public static ExtraDecorationDef GetArmorDecoDefFromString(string defName)
    {
        return DefDatabase<ExtraDecorationDef>.GetNamed(defName);
    }
    
    public static WeaponDecorationDef GetWeaponDecoDefFromString(string defName)
    {
        return DefDatabase<WeaponDecorationDef>.GetNamed(defName);
    }
        
    public static bool DeletePreset(Rect rect, ColourPreset preset)
    {
        rect.x += 5f;
        if (!Widgets.ButtonImage(rect, TexButton.Delete))
        {
            return false;
        }
            
        ModSettings.RemovePreset(preset);
        return true;
    }
    public static bool DeletePreset(Rect rect, ExtraDecorationPreset preset)
    {
        rect.x += 5f;
        if (!Widgets.ButtonImage(rect, TexButton.Delete))
        {
            return false;
        }
            
        ModSettings.RemovePreset(preset);
        return true;
    }
        
    //Colour Preview
    public static Texture2D ThreeColourPreview(Color primaryColor, Color? secondaryColor, Color? tertiaryColor, int colorAmount)
    {
        var texture2D = new Texture2D(3,3)
        {
            name = "SolidColorTex-" + primaryColor + secondaryColor
        };
        texture2D.SetPixel(0, 0, primaryColor);
        texture2D.SetPixel(0, 1, primaryColor);
        texture2D.SetPixel(0, 2, primaryColor);

        var secondRowPixel = primaryColor;
        var thirdRowPixel = primaryColor;
        
        if (secondaryColor.HasValue && secondaryColor.Value.a != 0 && colorAmount > 1)
        {
            secondRowPixel = secondaryColor.Value;
            thirdRowPixel = secondaryColor.Value;
        }
        if (tertiaryColor.HasValue && tertiaryColor.Value.a != 0 && colorAmount > 2)
        {
            thirdRowPixel = tertiaryColor.Value;
        }
        
        texture2D.SetPixel(1, 0, secondRowPixel);
        texture2D.SetPixel(1, 1, secondRowPixel);
        texture2D.SetPixel(1, 2, secondRowPixel);
        texture2D.SetPixel(2, 0, thirdRowPixel);
        texture2D.SetPixel(2, 1, thirdRowPixel);
        texture2D.SetPixel(2, 2, thirdRowPixel);
        texture2D.wrapMode = TextureWrapMode.Clamp;
        texture2D.filterMode = FilterMode.Point;
        texture2D.Apply();

        return texture2D;
    }
    
    public static bool ContainsAllItems<T>(this IEnumerable<T> a, IEnumerable<T> b)
    {
        return !b.Except(a).Any();
    }
    
    public static string ValueToString(StatDef stat, float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
    {
        if (!finalized)
        {
            var text = val.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense);
            if (numberSense != ToStringNumberSense.Factor && !stat.formatStringUnfinalized.NullOrEmpty())
            {
                text = string.Format(stat.formatStringUnfinalized, text);
            }
            return text;
        }
        var text2 = val.ToStringByStyle(stat.toStringStyle, numberSense);
        if (numberSense != ToStringNumberSense.Factor && !stat.formatString.NullOrEmpty())
        {
            text2 = string.Format(stat.formatString, text2);
        }
        return text2;
    }

    public static bool HasMultiColorThing(this Pawn pawn)
    {
        if (pawn.apparel?.WornApparel != null)
        {
            if (pawn.apparel.WornApparel.Any(apparel => apparel.HasComp<CompMultiColor>()))
            {
                return true;
            }
        }

        if (pawn.equipment?.Primary?.GetComp<CompMultiColor>() != null)
        {
            return true;
        }

        return false;
    }

    private static void SetupMultiColorCustomization(ThingWithComps thing, Dictionary<ColourPresetDef, ColorSelectionType> finalSelection, Pawn pawn)
    {
        var colorComp = thing.GetComp<CompMultiColor>();
        if (colorComp == null)
        {
            return;
        }
        
        var selection =
            finalSelection
                .Where(col => 
                    col.Value == ColorSelectionType.TryMatch 
                    && col.Key.appliesTo.Contains(thing.def.defName)).FirstOrFallback(new KeyValuePair<ColourPresetDef, ColorSelectionType>());

        if (selection.Key == null)
        {
            selection = finalSelection
                .Where(col => 
                    col.Value == ColorSelectionType.Default).FirstOrFallback();
        }

        if (selection.Key == null)
        {
            Log.Warning("Tried to give " + pawn.kindDef + " default colored clothe, but is not setup correctly");
            return;
        }
            
        colorComp.SetColors(selection.Key);
        colorComp.SetOriginals();
        colorComp.InitialSet = true;
    }
    
    private static void SetupDecorationCustomization(ThingWithComps thing, DefModExtension_PawnKindCustomization pawnModExtension)
    {
        var decoComp = thing.GetComp<CompDecorative>();
        if (decoComp == null || pawnModExtension == null)
        {
            return;
        }
        
        if (pawnModExtension.extraDecorationPreset.TryGetValue(thing.def, out var preset))
        {
            decoComp.LoadFromPreset(preset);
        }
        if (pawnModExtension.extraDecorations.TryGetValue(thing.def, out var decos))
        {
            decoComp.ApplyDecorationsFromList(decos);
        }
    }
    
    public static void SetupCustomizationForPawn(Pawn pawn, bool setupMultiColor, bool setupDecoration)
    {
        var factionSelection = pawn.Faction?.def?.GetModExtension<DefModExtension_PawnKindCustomization>()?.defaultColorSelection;
        var pawnModExtension = pawn.kindDef?.GetModExtension<DefModExtension_PawnKindCustomization>();
        var pawnKindSelection = pawnModExtension?.defaultColorSelection;
        
        Dictionary<ColourPresetDef, ColorSelectionType> finalSelection = null;
        if (!pawnKindSelection.NullOrEmpty())
        {
            finalSelection = pawnKindSelection;
        }
        else if (!factionSelection.NullOrEmpty())
        {
            finalSelection = factionSelection;
        }

        if (finalSelection == null)
        {
            return;
        }
        
        if (pawn.apparel?.WornApparel != null)
        {
            foreach (var apparel in pawn.apparel.WornApparel)
            {
                if (setupMultiColor)
                {
                    SetupMultiColorCustomization(apparel, finalSelection, pawn);
                }
                
                if (setupDecoration)
                {
                    SetupDecorationCustomization(apparel, pawnModExtension);
                }
            }
        }
        
        var equipment = pawn.equipment?.PrimaryEq?.parent;
        var colorCompWeap = equipment?.GetComp<CompMultiColor>();
        if (colorCompWeap != null)
        {
            if (setupMultiColor)
            {
                SetupMultiColorCustomization(equipment, finalSelection, pawn);
            }

            if (setupDecoration)
            {
                SetupDecorationCustomization(equipment, pawnModExtension);
            }
        }
    }

    public static int CountBuildingColonistOfDef(this ListerBuildings listerBuildings, ThingDef def)
    {
        return listerBuildings.AllBuildingsColonistOfDef(def).Count;
    }
    
    private static Color MenuSectionBGFillColor = new ColorInt(42, 43, 44).ToColor;
    private static Color MenuSectionBGBorderColor = new ColorInt(135, 135, 135).ToColor;
    
    public static void DrawColoredMenuSection(Rect rect, Color? menuFillColor, Color? borderColor)
    	{
    		GUI.color = menuFillColor ?? MenuSectionBGFillColor;
    		GUI.DrawTexture(rect, BaseContent.WhiteTex);
    		GUI.color = borderColor ?? MenuSectionBGBorderColor;
    		Widgets.DrawBox(rect);
    		GUI.color = Color.white;
    	}
}