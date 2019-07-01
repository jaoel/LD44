using UnityEngine;


public class Key : Item
{
    public CircleCollider2D trigger;
    public Rigidbody2D rigidBody;
    public GameObject visual;
    public GameObject minimapIcon;

    public Door Owner { get; set; }
    public bool Consumed { get; set; }
    public bool isGoldKey;

    public override void ApplyEffect(GameObject owner)
    {
        Player player = owner.GetComponent<Player>();
        player.AddKey(this, isGoldKey);

        Destroy(minimapIcon);
        Destroy(trigger);
        Destroy(rigidBody);
        Destroy(visual);
    }
}
