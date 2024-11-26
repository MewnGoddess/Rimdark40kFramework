using HarmonyLib;
using UnityEngine;
using Verse;


namespace Core40k
{
    public class Core40kModSettings : ModSettings
    {
        public bool alwaysShowRankTab = false;

        public bool showAllRankCategories = false;
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref alwaysShowRankTab, "alwaysShowRankTab", false);
            Scribe_Values.Look(ref showAllRankCategories, "showAllRankCategories", false);
            base.ExposeData();
        }
    }
}