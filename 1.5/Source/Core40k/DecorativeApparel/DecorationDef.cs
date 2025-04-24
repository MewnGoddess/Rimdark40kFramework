using System.Collections.Generic;
using Core40k;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k
{
    public class DecorationDef : Def
    {
        [NoTranslate]
        public string iconPath;
            
        [Unsaved]
        private Texture2D icon;
    
        public Texture2D Icon
        {
            get
            {
                if (icon != null)
                {
                    return icon;
                }
                    
                icon = !iconPath.NullOrEmpty() ? ContentFinder<Texture2D>.Get(iconPath) : ContentFinder<Texture2D>.Get("NoTex");
                return icon;
            }
        }
    
        [NoTranslate]
        public string drawnTextureIconPath;
        
        public float sortOrder = 0f;
        
        public Color defaultColour = Color.white;
        
        public List<DecorationColourPresetDef> availablePresets = new List<DecorationColourPresetDef>();
        
        public RankDef mustHaveRank = null;
        
        public GeneDef mustHaveGene = null;
    
        public TraitDef mustHaveTrait = null;
    
        public HediffDef mustHaveHediff = null;
        
        public bool HasRequirements(Pawn pawn)
        {
            if (mustHaveRank != null)
            {
                if (!pawn.HasComp<CompRankInfo>())
                {
                    return false;
                }
                var comp = pawn.GetComp<CompRankInfo>();
                if (!comp.HasRank(mustHaveRank))
                {
                    return false;
                }
            }
    
            /*if (mustHaveGene != null)
            {
                
            }
            
            if (mustHaveTrait != null)
            {
                
            }
            
            if (mustHaveHediff != null)
            {
                
            }*/
            
            return true;
        }
    }
}