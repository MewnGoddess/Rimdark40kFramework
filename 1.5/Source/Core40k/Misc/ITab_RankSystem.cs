using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k
{
    public class ITab_RankSystem : ITab
    {
        private RankDef currentlySelectedRank = null;
        
        private RankCategoryDef currentlySelectedRankCategory = null;
        
        private List<RankCategoryDef> availableCategories = new List<RankCategoryDef>();
        
        public override bool IsVisible
        {
            get
            {
                //Go through each rankCategory and check if they are able to "use" at least on of them
                return Find.Selector.SingleSelectedThing is Pawn pawn && (pawn.Faction?.IsPlayer ?? false);
            }
        }
        
        public ITab_RankSystem()
        {
            size = new Vector2(UI.screenWidth, UI.screenHeight * 0.75f);
            labelKey = "BEWH.RankTab";
        }

        protected override void FillTab()
        {
            //For internal size:
            //Check highest and lowest y value among ranks, and multiply some size (for each icon + some leway) and
            //Check for higest x valuye and do same as above
            return;
        }

        private void UpdateRankCategoryList()
        {
            
        }
    }
}