using System;


[Serializable]
public class DroppableItem
{
    public Item item;
    public float droprate;
    public int minDropCount;
    public int maxDropCount;
    public bool useSeclusionFactor;
}
