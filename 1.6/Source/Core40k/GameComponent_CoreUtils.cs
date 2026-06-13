using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Core40k;

public class GameComponent_CoreUtils : GameComponent
{
    /*private List<Pawn> pawnList = [];
    private List<CachedDecoratives> cachedList = [];*/
    public Dictionary<Pawn, CachedDecoratives> cachedDecoratives = new ();
    
    /*private List<Pawn> pawnList2 = [];
    private List<CachedDecoratives> cachedList2 = [];*/
    public Dictionary<Pawn, CachedDecoratives> cachedAlternateTexture = new ();
    
    public GameComponent_CoreUtils(Game game)
    {
    }

    /*public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref cachedDecoratives, "cachedDecoratives", LookMode.Reference, LookMode.Deep, ref pawnList, ref cachedList);
        Scribe_Collections.Look(ref cachedAlternateTexture, "cachedMultiColor", LookMode.Reference, LookMode.Deep, ref pawnList2, ref cachedList2);
        
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        if (!pawnList.NullOrEmpty())
        {
            return;
        }
        
        cachedDecoratives = new Dictionary<Pawn, CachedDecoratives>();
        cachedAlternateTexture = new Dictionary<Pawn, CachedDecoratives>();
        pawnList = [];
        cachedList = [];
        pawnList2 = [];
        cachedList2 = [];
    }*/
    
    public class CachedDecoratives : IExposable
    {
        public List<Apparel> apparels = [];
        public ThingWithComps weapon;
        
        public void ExposeData()
        {
            Scribe_Collections.Look(ref apparels, "apparels", LookMode.Reference);
            Scribe_References.Look(ref weapon, "weapon");

        }
    }
}