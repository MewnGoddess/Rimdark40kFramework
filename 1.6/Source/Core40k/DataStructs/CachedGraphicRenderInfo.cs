using UnityEngine;
using Verse;

namespace Core40k;

public struct CachedGraphicRenderInfo
{
    public Graphic graphic;

    public int layer;

    public Vector3 scale;

    public Vector3 positionOffset;

    public CachedGraphicRenderInfo(Graphic graphic, int layer, Vector3 scale, Vector3 positionOffset)
    {
        this.graphic = graphic;
        this.layer = layer;
        this.scale = scale;
        this.positionOffset = positionOffset;
    }
}