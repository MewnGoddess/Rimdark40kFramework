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
    private static Core40kModSettings modSettings = null;
    public static Core40kModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<Core40kMod>().GetSettings<Core40kModSettings>();

    public static ExtraDecorationDef GetDefFromString(string defName)
    {
        return DefDatabase<ExtraDecorationDef>.GetNamed(defName);
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
        texture2D.filterMode = FilterMode.Bilinear;
        texture2D.Apply();

        return texture2D;
    }
}