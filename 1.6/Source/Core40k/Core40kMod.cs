﻿using HarmonyLib;
using UnityEngine;
using Verse;

namespace Core40k;

public class Core40kMod : Mod
{
    public static Harmony harmony;
        
    readonly Core40kModSettings settings;
    public Core40kMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<Core40kModSettings>();
        harmony = new Harmony("Core40k.Mod");
        harmony.PatchAll();
    }
        
    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);

        listingStandard.CheckboxLabeled("BEWH.Framework.ModSettings.ShowRankTab".Translate(), ref settings.alwaysShowRankTab);
        
        listingStandard.CheckboxLabeled("BEWH.Framework.ModSettings.ConfirmRankUnlock".Translate(), ref settings.confirmRankUnlock);

        listingStandard.Label("\n" + "BEWH.ModSettings.CheckVEFPatches".Translate());
            
        listingStandard.End();
        base.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "BEWH.Framework.ModSettings.ModName".Translate();
    }
}