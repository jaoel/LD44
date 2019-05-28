using UnityEngine;

public class Key : MonoBehaviour
{
    public CircleCollider2D trigger;
    public Rigidbody2D rigidBody;
    public GameObject visual;
    public GameObject minimapIcon;
    public AudioSource pickupSound;

    public Door Owner { get; set; }
    public bool Consumed { get; set; }
    public bool isGoldKey;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.AddKey(this, isGoldKey);

            pickupSound.volume = SettingsManager.Instance.SFXVolume;
            pickupSound.Play();

            Destroy(minimapIcon);
            Destroy(trigger);
            Destroy(rigidBody);
            Destroy(visual);
        }
    }
}
