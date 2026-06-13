using UnityEngine;
using Verse;

namespace Core40k;

public class ModSettingTab_CoreCustomization : ModSettingTab
{
    public override void DrawTab(Rect inRect, ModSettings settings)
    {
        if (settings is not Core40kModSettings core40KModSettings)
        {
            Log.Error("Settings not correct type");
            return;
        }
        
        var viewRect = new Rect(inRect.x, inRect.y, inRect.width - 16f, scrollViewHeight);
        scrollViewHeight = 0f;
            
        Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(viewRect);
        listingStandard.Gap(36);
        scrollViewHeight += ListingHeightIncreaseGap;
        scrollViewHeight += ListingHeightIncrease;
        
        listingStandard.CheckboxLabeled("BEWH.Framework.ModSettings.ShowCustomizationDebugOptions".Translate(), ref core40KModSettings.showCustomizationDebugOptions);
        scrollViewHeight += ListingHeightIncrease;
        
        core40KModSettings.decorationsPerRow = (int)listingStandard.SliderLabeled("BEWH.Framework.ModSettings.DecorationsPerRow".Translate(core40KModSettings.decorationsPerRow),core40KModSettings.decorationsPerRow, 3, 8, tooltip: "BEWH.Framework.ModSettings.DecorationsPerRowTooltip".Translate());
        scrollViewHeight += ListingHeightIncrease;
        
        //Check VEF patches
        listingStandard.GapLine(36);
        scrollViewHeight += ListingHeightIncreaseGap;
        listingStandard.Label("\n" + "BEWH.ModSettings.CheckVEFPatches".Translate());
        scrollViewHeight += ListingHeightIncrease;
        
        scrollViewHeight += ListingHeightIncrease;
        
        listingStandard.End();
        Widgets.EndScrollView();
    }
}