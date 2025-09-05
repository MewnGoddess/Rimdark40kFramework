using System.Collections.Generic;
using System.Linq;
using Core40k;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

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
    
    public DrawData drawData = new();
    public ShaderTypeDef shaderType;
        
    public Color? defaultColour;
    public Color? defaultColourTwo;
    public Color? defaultColourThree;
        
    public List<DecorationColourPresetDef> availablePresets = new();
        
    public List<RankDef> mustHaveRank = null;
        
    public List<GeneDef> mustHaveGene = null;
    
    public List<TraitData> mustHaveTrait = null;
    
    public List<HediffDef> mustHaveHediff = null;
    
    public List<DecorationFlag> decorationFlags = new();
    
    public virtual bool HasRequirements(Pawn pawn)
    {
        if (mustHaveRank != null)
        {
            if (!pawn.HasComp<CompRankInfo>())
            {
                return false;
            }
            var comp = pawn.GetComp<CompRankInfo>();
            if (mustHaveRank.All(rank => !comp.HasRank(rank)))
            {
                return false;
            }
        }
    
        if (mustHaveGene != null)
        {
            if (pawn.genes == null)
            {
                return false;
            }
            
            if (mustHaveGene.All(gene => !pawn.genes.HasActiveGene(gene)))
            {
                return false;
            }
        }

        if (mustHaveTrait != null)
        {
            if (pawn.story?.traits == null)
            {
                return false;
            }
            
            if (mustHaveTrait.All(trait => !pawn.story.traits.HasTrait(trait.traitDef, trait.degree)))
            {
                return false;
            }
        }

        if (mustHaveHediff != null)
        {
            if (pawn.health?.hediffSet == null)
            {
                return false;
            }
            
            if (mustHaveHediff.All(hediff => !pawn.health.hediffSet.HasHediff(hediff)))
            {
                return false;
            }
        }
            
        return true;
    }
    
    public override void ResolveReferences()
    {
        base.ResolveReferences();
        shaderType ??= Core40kDefOf.BEWH_CutoutThreeColor;
    }
}

public class DecorationFlag
{
    public string flag;
    public string newTexPath;
    public string maskPathAddition;
    public ShaderTypeDef shaderType = null;
    public int priority = 0;
}