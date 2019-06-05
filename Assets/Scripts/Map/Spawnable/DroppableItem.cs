using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[Serializable]
public class DroppableItem
{
    public ItemDescription item;
    public float droprate;
    public int minDropCount;
    public int maxDropCount;
    public bool useSeclusionFactor;
}
