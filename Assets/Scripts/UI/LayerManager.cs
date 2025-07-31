using UnityEngine;

/// <summary>
/// Conveniant layer management...
/// </summary>
public static class LayerManager
{
    public static class Layers
    {
        public static int DEFAULT = 0;
        public static int PLAYER = 6;
        public static int NPC = 7;
    }

    public static class SortingLayers
    {
        public static int DEFAULT = SortingLayer.NameToID("Default");
    }

    public static class Masks
    {
        public static LayerMask DEFAULT = 1 << Layers.DEFAULT;
        public static LayerMask PLAYER = 1 << Layers.PLAYER;
        public static LayerMask NPC = 1 << Layers.NPC;
    }

    public static int ToLayerID(LayerMask layerMask)
    {
        return Mathf.RoundToInt(Mathf.Log(layerMask.value, 2));
    }
}