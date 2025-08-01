using UnityEngine;

/// <summary>
/// Conveniant layer management...
/// </summary>
public static class LayerManager
{
    public static class Layers
    {
        public static int DEFAULT = 0;
        public static int LEVEL = 3;
        public static int PLAYER = 6;
        public static int ENEMY = 7;
        public static int ASTEROID = 8;
    }

    public static class SortingLayers
    {
        public static int DEFAULT = SortingLayer.NameToID("Default");
    }

    public static class Masks
    {
        public static LayerMask DEFAULT = 1 << Layers.DEFAULT;
        public static LayerMask LEVEL = 1 << Layers.LEVEL;
        public static LayerMask PLAYER = 1 << Layers.PLAYER;
        public static LayerMask ENEMY = 1 << Layers.ENEMY;
        public static LayerMask ASTEROID = 1 << Layers.ASTEROID;
    }

    public static int ToLayerID(LayerMask layerMask)
    {
        return Mathf.RoundToInt(Mathf.Log(layerMask.value, 2));
    }
}