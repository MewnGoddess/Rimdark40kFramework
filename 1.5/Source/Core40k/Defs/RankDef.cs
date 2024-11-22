using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Core40k
{
    public class RankDef : Def
    {
        public RankCategoryDef rankCategory;

        public List<RankDef> rankRequirements;
        
        public Vector2 diplayPosition; //Goes from 5 to -5 on y axis and 0 to a lot on x axis
    }
}