using UnityEngine;

public class FiringRateModifier : Item
{
    public float firingRateChange;
    public override void ApplyEffect(GameObject owner)
    {
        owner.GetComponent<Player>().firingRateModifier += firingRateChange;
    }
} 
