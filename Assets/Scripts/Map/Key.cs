using UnityEngine;

public class Key : MonoBehaviour
{
    public CircleCollider2D trigger;
    public Rigidbody2D rigidBody;
    public GameObject visual;

    public Door Owner { get; set; }
    public bool Consumed { get; set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.AddKey(this);

            Destroy(trigger);
            Destroy(rigidBody);
            Destroy(visual);
        }
    }
}
