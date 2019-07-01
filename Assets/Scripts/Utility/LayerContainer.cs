using UnityEngine;

public static class Layers
{
    public static int Map = LayerMask.NameToLayer("Map");
    public static int Pits = LayerMask.NameToLayer("Pits");
    public static int Player = LayerMask.NameToLayer("Player");
    public static int FriendlyBullet = LayerMask.NameToLayer("FriendlyBullet");
    public static int Enemy = LayerMask.NameToLayer("Enemy");
    public static int FlyingEnemy = LayerMask.NameToLayer("FlyingEnemy");
    public static int Corpse = LayerMask.NameToLayer("Corpse");

    public static int CombinedLayerMask(params int[] layers)
    {
        int result = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            result |= 1 << layers[i];
        }

        return result;
    }
}
