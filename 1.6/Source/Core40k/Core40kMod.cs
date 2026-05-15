using HarmonyLib;
using UnityEngine;
using Verse;

namespace Core40k;

public class Core40kMod : CoreMod
{
    public static string CurrentVersion;
    
    public static Harmony harmony;
        
    private Core40kModSettings settings;
    public override ModSettings Settings => settings ??= GetSettings<Core40kModSettings>();
    public Core40kMod(ModContentPack content) : base(content)
    {
        harmony = new Harmony("Core40k.Mod");
        CurrentVersion = content.ModMetaData.ModVersion;
        
        harmony.PatchAll();
    }
    
    private readonly ModSettingTab_CoreMain coreMainSettings = new();
    
    public override void InitializeTabs()
    {
        var mainTab = new TabRecord("BEWH.ModSettings.TabMain".Translate(), delegate
        {
            currentSettingTab = coreMainSettings;
        }, () => currentSettingTab == coreMainSettings);
        tabs.Add(mainTab);

        currentSettingTab = coreMainSettings;
        base.InitializeTabs();
    }

    public override string SettingsCategory()
    {
        return "BEWH.Framework.ModSettings.ModName".Translate(CurrentVersion);
    }
}