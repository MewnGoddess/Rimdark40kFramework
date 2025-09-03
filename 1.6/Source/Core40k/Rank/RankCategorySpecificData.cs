using System.Collections.Generic;
using UnityEngine;

namespace Core40k;

public class RankCategorySpecificData
{
    public RankDef rankDef;
    public Vector2 displayPosition;
    public List<RankData> rankRequirements = new List<RankData>();
    public List<RankData> rankRequirementsOneAmong = new List<RankData>();
}